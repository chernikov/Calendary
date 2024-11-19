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

    public DbSet<PromptTheme> PromptThemes { get; set; }

    public DbSet<Role> Roles { get; set; }

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
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<CalendarHoliday>()
            .HasKey(ur => new { ur.CalendarId, ur.HolidayId });

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

        modelBuilder.Entity<Role>().HasData(
            Role.AdminRole,
            Role.UserRole
        );

        modelBuilder.Entity<Language>().HasData(
            Language.Ukrainian,
            Language.English
        );

        modelBuilder.Entity<Country>().HasData(
            Country.Ukraine
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Identity = Guid.Parse("64F39BD1-633C-4B82-A64C-04CA94B77E90"),
                UserName = "admin",
                Email = "admin@calendary.com.ua",
                PasswordHash = "21232f297a57a5a743894a0e4a801fc3",
                IsEmailConfirmed = true,
                IsPhoneNumberConfirmed = true,
                IsTemporary = false,
                Created = new DateTime(2024, 10, 15),
            }
        );

        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                UserId = 1,
                RoleId = 1
            }
        );

        modelBuilder.Entity<OrderItem>()
        .Property(o => o.Price)
        .HasColumnType("decimal(18, 2)");
    }
}