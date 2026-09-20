using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calendary.Api.Tests;

// Runs the real ASP.NET Core pipeline (routing, auth, MediatR, exception filters — everything
// Program.cs wires up) against an in-memory SQLite database instead of the real SQL Server one, so
// these tests need no Docker/DB to run and stay fast in CI. Program.cs itself picks Sqlite over
// SqlServer when the environment is "Testing" (see its AppDbContext registration) — a second
// AddDbContext call from here that tried to swap the provider post-hoc doesn't work, since EF
// Core 8+ chains AddDbContext configuration callbacks rather than replacing them, which ends up
// registering both providers and throwing at startup.
//
// SQLite's in-memory database only exists as long as its one connection stays open, and a plain
// connection-string DataSource opens a fresh (empty) connection per request — so a single open
// SqliteConnection is registered as the DbConnection singleton Program.cs's AppDbContext
// registration resolves and reuses for every request, for the whole factory's lifetime.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Program.cs throws at startup if this is missing — the real value is never used in
            // the Testing environment, since its AppDbContext registration ignores it in favor of
            // the DbConnection singleton registered below.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "unused",
            });
        });
        builder.ConfigureServices(services => services.AddSingleton<DbConnection>(_connection));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
