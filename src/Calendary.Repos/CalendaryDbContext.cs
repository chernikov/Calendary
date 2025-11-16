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

    public DbSet<HolidayPreset> HolidayPresets { get; set; }

    public DbSet<HolidayPresetTranslation> HolidayPresetTranslations { get; set; }

    public DbSet<HolidayTranslation> HolidayTranslations { get; set; }

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

    // Customer Portal entities
    public DbSet<Template> Templates { get; set; }

    public DbSet<UserCalendar> UserCalendars { get; set; }

    public DbSet<CartItem> CartItems { get; set; }

    public DbSet<UploadedFile> UploadedFiles { get; set; }

    // Credits system entities
    public DbSet<Credit> Credits { get; set; }

    public DbSet<CreditTransaction> CreditTransactions { get; set; }

    public DbSet<CreditPackage> CreditPackages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalendaryDbContext).Assembly);

        // Seed initial data
        DbSeeder.SeedData(modelBuilder);
    }
}