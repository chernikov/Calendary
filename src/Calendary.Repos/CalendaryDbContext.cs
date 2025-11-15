using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos;
public class CalendaryDbContext : DbContext, ICalendaryDbContext
{
    public CalendaryDbContext(DbContextOptions<CalendaryDbContext> options) : base(options)
    {
    }
    public DbSet<CalendarHoliday> CalendarHolidays { get; set; }

    public DbSet<Calendar> Calendars { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Country> Countries { get; set; }

    public DbSet<EventDate> EventDates { get; set; }

    public DbSet<FluxModel> FluxModels { get; set; }

    public DbSet<Holiday> Holidays { get; set; }

    public DbSet<Image> Images { get; set; }

    public DbSet<Job> Jobs { get; set; }

    public DbSet<JobTask> JobTasks { get; set; }

    public DbSet<Language> Languages { get; set; }

    public DbSet<MonoWebhookEvent> MonoWebhookEvents { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<PaymentInfo> PaymentInfos { get; set; }

    public DbSet<Photo> Photos { get; set; }

    public DbSet<Prompt> Prompts { get; set; }

    public DbSet<PromptSeed> PromptSeeds { get; set; }

    public DbSet<PromptTheme> PromptThemes { get; set; }

    public DbSet<ResetToken> ResetTokens { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Synthesis> Synthesises { get; set; }

    public DbSet<Training> Trainings { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserSetting> UserSettings { get; set; }

    public DbSet<VerificationEmailCode> VerificationEmailCodes { get; set; }

    public DbSet<VerificationPhoneCode> VerificationPhoneCodes { get; set; }

    public DbSet<WebHook> WebHooks { get; set; }

    public DbSet<WebHookFluxModel> WebHookFluxModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure composite keys
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<CalendarHoliday>()
            .HasKey(ur => new { ur.CalendarId, ur.HolidayId });

        // Configure relationships
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<CalendarHoliday>()
            .HasOne(ur => ur.Calendar)
            .WithMany(u => u.CalendarHolidays)
            .HasForeignKey(ur => ur.CalendarId);

        modelBuilder.Entity<EventDate>()
            .HasOne(ed => ed.UserSetting)
            .WithMany(us => us.EventDates)
            .HasForeignKey(ed => ed.UserSettingId);

        modelBuilder.Entity<Holiday>()
            .HasOne(h => h.Country)
            .WithMany(c => c.Holidays)
            .HasForeignKey(h => h.CountryId);

        // Configure column types
        modelBuilder.Entity<OrderItem>()
            .Property(o => o.Price)
            .HasColumnType("decimal(18, 2)");

        // Seed initial data
        DbSeeder.SeedData(modelBuilder);
    }
}