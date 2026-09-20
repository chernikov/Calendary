using Calendary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Common;

/// Lets Application query/persist without depending on Calendary.Infrastructure's concrete EF
/// AppDbContext (Application → Infrastructure would invert Clean Architecture's dependency
/// direction). Lives here rather than in Domain because it needs the DbSet&lt;T&gt; type from EF
/// Core, and Domain is kept free of any EF/ASP.NET dependency — Application has no such
/// constraint. Mirrors every DbSet the real AppDbContext declares, not just the ones any one
/// feature touches today, so it's one stable interface rather than something to keep expanding
/// piecemeal per feature.
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }
    DbSet<Sheet> Sheets { get; }
    DbSet<SheetVariant> SheetVariants { get; }
    DbSet<OrderPhoto> OrderPhotos { get; }
    DbSet<PromptTheme> PromptThemes { get; }
    DbSet<Prompt> Prompts { get; }
    DbSet<ImageStyle> ImageStyles { get; }
    DbSet<PersonalDate> PersonalDates { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<PromoCode> PromoCodes { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Delivery> Deliveries { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<AppSettings> AppSettings { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
