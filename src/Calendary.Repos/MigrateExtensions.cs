using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Calendary.Repos;

public static class MigrateExtensions
{
    public static void ApplyMigrationDb(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CalendaryDbContext>();
            dbContext.Database.Migrate();
            
            // Seed demo користувачів після міграції
            dbContext.SeedDemoUsers();
        }
    }
}
