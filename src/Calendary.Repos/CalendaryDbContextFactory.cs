using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos;

public class CalendaryDbContextFactory : IDesignTimeDbContextFactory<CalendaryDbContext>
{
    public CalendaryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CalendaryDbContext>();

        // Замініть на ваш рядок підключення
        optionsBuilder.UseSqlServer("Server=IRON;Database=CalendaryDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");

        return new CalendaryDbContext(optionsBuilder.Options);
    }
}