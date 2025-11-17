using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos;

public class CalendaryDbContextFactory : IDesignTimeDbContextFactory<CalendaryDbContext>
{
    public CalendaryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CalendaryDbContext>();
        optionsBuilder.UseSqlServer("Server=IRON;Database=calendary_db;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");

        return new CalendaryDbContext(optionsBuilder.Options);
    }
}