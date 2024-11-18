using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos;

public interface ICalendaryDbContext
{
    DbSet<CalendarHoliday> CalendarHolidays { get; set; }
    DbSet<Calendar> Calendars { get; set; }
    DbSet<Country> Countries { get; set; }
    DbSet<EventDate> EventDates { get; set; }
    DbSet<Holiday> Holidays { get; set; }
    DbSet<Image> Images { get; set; }
    DbSet<Language> Languages { get; set; }
    DbSet<MonoWebhookEvent> MonoWebhookEvents { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    DbSet<PaymentInfo> PaymentInfos { get; set; }
    DbSet<Role> Roles { get; set; }
    DbSet<UserRole> UserRoles { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<UserSetting> UserSettings { get; set; }
    DbSet<VerificationEmailCode> VerificationEmailCodes { get; set; }
    DbSet<VerificationPhoneCode> VerificationPhoneCodes { get; set; }

    DbSet<WebHook> WebHooks { get; set; }



    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}