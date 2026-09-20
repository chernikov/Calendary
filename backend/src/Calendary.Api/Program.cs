using System.Threading.RateLimiting;
using Calendary.AI;
using Calendary.Api.Auth;
using Calendary.Application.Common;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Options;
using Calendary.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

// Centralizes the "business operation" logging #300 asks for (order status, payment attempt/
// success/failure, per-sheet generation start/finish/fail) by hooking SaveChanges instead of
// touching every one of the ~20 call sites that mutate these entities — see the interceptor's own
// doc comment.
builder.Services.AddSingleton<DomainStatusLoggingInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) => options
    .UseSqlServer(connectionString)
    .AddInterceptors(sp.GetRequiredService<DomainStatusLoggingInterceptor>()));
// Calendary.Application depends on this instead of the concrete AppDbContext, so it never has to
// reference Calendary.Infrastructure (see #298).
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Calendary.Application.AssemblyReference).Assembly));

builder.Services.AddScoped<IImageGenerationService, DynamicImageGenerationService>();
builder.Services.AddSingleton<IPhotoThumbnailGenerator, PhotoThumbnailGeneratorService>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddCalendaryAi(builder.Configuration);
builder.Services.AddHttpClient<IPaymentService, MonobankPaymentService>();
builder.Services.AddHttpClient<INovaPoshtaService, NovaPoshtaService>();
builder.Services.AddHttpClient<ICalendarPdfService, CalendarPdfService>();
builder.Services.AddHttpClient<ISmsService, SmsClubService>();
builder.Services.Configure<SmsClubOptions>(builder.Configuration.GetSection(SmsClubOptions.SectionName));
builder.Services.AddScoped<ISessionTokenService, SessionTokenService>();
builder.Services.AddScoped<IPasswordAuthService, PasswordAuthService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection(GoogleOptions.SectionName));
builder.Services.AddHttpClient<IEmailService, ResendEmailService>();
builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection(ResendOptions.SectionName));
builder.Services.Configure<MonobankOptions>(builder.Configuration.GetSection(MonobankOptions.SectionName));
builder.Services.Configure<NovaPoshtaOptions>(builder.Configuration.GetSection(NovaPoshtaOptions.SectionName));
builder.Services.Configure<BackupOptions>(builder.Configuration.GetSection(BackupOptions.SectionName));
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.AddScoped<IBackupStatusService, ResticBackupStatusService>();

builder.Services.AddHostedService<FulfillmentBackgroundService>();
builder.Services.AddHostedService<GenerationBackgroundService>();
builder.Services.AddHostedService<OrderExpiryBackgroundService>();
builder.Services.AddHostedService<UserSessionCleanupBackgroundService>();

builder.Services.AddAuthentication(BearerTokenAuth.Scheme)
    .AddScheme<AuthenticationSchemeOptions, BearerTokenAuthenticationHandler>(BearerTokenAuth.Scheme, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200"])
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// #300: lets Docker/Caddy tell a live container from a hung one, and docker-compose's
// depends_on: condition: service_healthy wait for the backend to actually be ready (DB reachable)
// before the frontend/edge starts routing to it.
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// #301: brute-force/enumeration protection for register/login/google/forgot-password/reset-
// password (see AuthController's [EnableRateLimiting("auth")]). Partitioned per client IP — this
// only works correctly once ForwardedHeaders (below) has resolved the real client IP instead of
// the frontend nginx container's, since every request reaches this backend through that proxy.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));

    // #304: real SMS.Club sends cost money and reach a real phone (no sandbox mode) — a much
    // tighter cap than "auth", partitioned per authenticated user rather than IP since this sits
    // behind [Authorize]. app.UseRateLimiter() runs after UseAuthentication/UseAuthorization
    // (below), so User is already populated by the time this factory runs.
    options.AddPolicy("sms", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.User.GetUserId().ToString(),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
        }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    await AdminSeeder.EnsureAdminUserAsync(
        db,
        scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value,
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>());

    await MediaMigrator.ConvertInlineImagesAsync(
        db,
        scope.ServiceProvider.GetRequiredService<IFileStorage>(),
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>());
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var fileStorageOptions = builder.Configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
    ?? new FileStorageOptions();
var mediaRoot = fileStorageOptions.ResolveRootPath(app.Environment.ContentRootPath);
Directory.CreateDirectory(mediaRoot);

// Every request reaches this backend through the frontend nginx container's proxy_pass (see
// nginx.conf), and in prod/staging through Caddy's reverse_proxy in front of that — so without
// this, Connection.RemoteIpAddress is always that proxy's own docker-internal IP, the same for
// every request, which would make the "auth" rate limiter above throttle the whole site as one
// client instead of per real visitor. KnownNetworks/KnownProxies are cleared (not left at their
// loopback-only default) because backend/mssql are never exposed directly to the internet (see
// CLAUDE.md) — the only thing that can reach this backend at all is that trusted internal hop, so
// trusting whatever it forwards is safe. ForwardLimit is unset (unlimited) since the hop count
// differs by environment: nginx only locally, nginx+Caddy in prod/staging.
var forwardedHeadersOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor };
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseCors();

// Filenames are unguessable GUIDs, so the URLs act as capability tokens and need no auth check —
// which also lets the browser cache them like any other image.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = fileStorageOptions.PublicBasePath,
    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    {
        [".jpg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
    }),
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable",
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
