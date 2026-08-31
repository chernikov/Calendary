using Calendary.AI;
using Calendary.Api.Auth;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Options;
using Calendary.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IImageGenerationService, DynamicImageGenerationService>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddCalendaryAi(builder.Configuration);
builder.Services.AddScoped<IPaymentService, MockPaymentService>();
builder.Services.AddSingleton<INovaPoshtaService, MockNovaPoshtaService>();
builder.Services.AddHttpClient<ICalendarPdfService, CalendarPdfService>();
builder.Services.AddScoped<ISessionTokenService, SessionTokenService>();
builder.Services.AddScoped<IPasswordAuthService, PasswordAuthService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection(GoogleOptions.SectionName));
builder.Services.AddHttpClient<IEmailService, ResendEmailService>();
builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection(ResendOptions.SectionName));

builder.Services.AddHostedService<FulfillmentBackgroundService>();
builder.Services.AddHostedService<GenerationBackgroundService>();

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

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

app.MapControllers();

app.Run();
