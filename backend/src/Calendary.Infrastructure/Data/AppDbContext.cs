using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Sheet> Sheets => Set<Sheet>();
    public DbSet<StyleCategory> StyleCategories => Set<StyleCategory>();
    public DbSet<PersonalDate> PersonalDates => Set<PersonalDate>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        modelBuilder.Entity<User>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSession>()
            .HasIndex(s => s.TokenHash)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Delivery)
            .WithOne(d => d.Order)
            .HasForeignKey<Delivery>(d => d.OrderId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Sheets)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.PersonalDates)
            .WithOne(d => d.Order)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(o => o.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<StyleCategory>().HasData(
            new StyleCategory { Id = Guid.Parse("11111111-1111-1111-1111-111111111101"), Code = "history", Name = "Історія", Description = "вікінг, фараон, самурай, лицар, козак…", SortOrder = 1 },
            new StyleCategory { Id = Guid.Parse("11111111-1111-1111-1111-111111111102"), Code = "cinema", Name = "Кіно", Description = "нуар, вестерн, шпигун, мюзикл…", SortOrder = 2 },
            new StyleCategory { Id = Guid.Parse("11111111-1111-1111-1111-111111111103"), Code = "adventure", Name = "Пригоди", Description = "альпініст, пілот, дайвер, полярник…", SortOrder = 3 },
            new StyleCategory { Id = Guid.Parse("11111111-1111-1111-1111-111111111104"), Code = "professions", Name = "Професії", Description = "шеф, лікар, диригент, пожежник…", SortOrder = 4 }
        );

        modelBuilder.Entity<AppSettings>().HasData(
            new AppSettings
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                ImageGenerationProvider = ImageGenerationProvider.OpenAI
            }
        );
    }
}
