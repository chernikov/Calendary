using Calendary.Repos;
using Calendary.Core;
using Microsoft.EntityFrameworkCore;
using Calendary.Api;
using Calendary.Core.Services;

var builder = WebApplication.CreateBuilder(args);


// Retrieve the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new Exception("Can't find connection string with name DefaultConnection");
builder.Services.AddCalendaryRepositories(connectionString);

builder.Services.RegisterJwtAuthentication(builder.Configuration);
builder.Services.RegisterPathProvider();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddCoreServices();


// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();