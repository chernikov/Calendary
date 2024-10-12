using Calendary.Repos;
using Calendary.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Retrieve the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new Exception("Can't find connection string with name DefaultConnection");

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddCalendaryRepositories(connectionString);
builder.Services.AddServices();

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();