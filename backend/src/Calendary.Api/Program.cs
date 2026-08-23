using Calendary.Api.Auth;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IImageGenerationService, MockImageGenerationService>();
builder.Services.AddScoped<IPaymentService, MockPaymentService>();
builder.Services.AddSingleton<INovaPoshtaService, MockNovaPoshtaService>();
builder.Services.AddScoped<IDevAuthService, DevAuthService>();

builder.Services.AddHostedService<GenerationBackgroundService>();
builder.Services.AddHostedService<FulfillmentBackgroundService>();

builder.Services.AddAuthentication(DevTokenAuth.Scheme)
    .AddScheme<AuthenticationSchemeOptions, DevTokenAuthenticationHandler>(DevTokenAuth.Scheme, _ => { });
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
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
