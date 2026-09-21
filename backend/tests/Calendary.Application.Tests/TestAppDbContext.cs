using Calendary.Application.Common;
using Calendary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Tests;

// A minimal EF Core InMemory-backed IAppDbContext for testing Application handlers in isolation —
// Application never references Infrastructure (see CLAUDE.md), so its tests can't reuse the real
// AppDbContext either; this mirrors just the DbSets IAppDbContext declares.
public class TestAppDbContext(DbContextOptions<TestAppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Sheet> Sheets => Set<Sheet>();
    public DbSet<SheetVariant> SheetVariants => Set<SheetVariant>();
    public DbSet<OrderPhoto> OrderPhotos => Set<OrderPhoto>();
    public DbSet<PromptTheme> PromptThemes => Set<PromptTheme>();
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<ImageStyle> ImageStyles => Set<ImageStyle>();
    public DbSet<PersonalDate> PersonalDates => Set<PersonalDate>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    public static TestAppDbContext Create() =>
        new(new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    // Sheet has two relationships to SheetVariant (Variants collection + ActiveVariant single
    // pointer) — EF's convention can't disambiguate those without explicit configuration, same as
    // the real AppDbContext.OnModelCreating.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sheet>()
            .HasMany(s => s.Variants)
            .WithOne(v => v.Sheet)
            .HasForeignKey(v => v.SheetId);

        modelBuilder.Entity<Sheet>()
            .HasOne(s => s.ActiveVariant)
            .WithMany()
            .HasForeignKey(s => s.ActiveVariantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
