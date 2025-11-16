using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos;

public interface ICalendaryDbContext
{
    DbSet<CalendarHoliday> CalendarHolidays { get; set; }
    DbSet<Calendar> Calendars { get; set; }

    DbSet<Category> Categories { get; set; }
    DbSet<Country> Countries { get; set; }
    DbSet<EventDate> EventDates { get; set; }
    DbSet<FluxModel> FluxModels { get; set; }
    DbSet<Holiday> Holidays { get; set; }
    DbSet<HolidayPreset> HolidayPresets { get; set; }
    DbSet<HolidayPresetTranslation> HolidayPresetTranslations { get; set; }
    DbSet<HolidayTranslation> HolidayTranslations { get; set; }
    DbSet<Image> Images { get; set; }
    DbSet<Job> Jobs { get; set; }
    DbSet<JobTask> JobTasks { get; set; }
    DbSet<Language> Languages { get; set; }
    DbSet<MonoWebhookEvent> MonoWebhookEvents { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    DbSet<PaymentInfo> PaymentInfos { get; set; }
    DbSet<Photo> Photos { get; set; }
    DbSet<Prompt> Prompts { get; set; }
    DbSet<PromptSeed> PromptSeeds { get; set; }
    DbSet<PromptTheme> PromptThemes { get; set; }
    DbSet<ResetToken> ResetTokens { get; set; }

    DbSet<Role> Roles { get; set; }

    DbSet<Synthesis> Synthesises { get; set; }
    DbSet<Training> Trainings { get; set; }
    DbSet<UserRole> UserRoles { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<UserSetting> UserSettings { get; set; }
    DbSet<VerificationEmailCode> VerificationEmailCodes { get; set; }
    DbSet<VerificationPhoneCode> VerificationPhoneCodes { get; set; }
    DbSet<WebHook> WebHooks { get; set; }
    DbSet<WebHookFluxModel> WebHookFluxModels { get; set; }

    /// <summary>
    /// Зберігає всі зміни у базі даних.
    /// </summary>
    /// <param name="cancellationToken">Токен для відміни операції.</param>
    /// <returns>Кількість змінених записів у базі даних.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}