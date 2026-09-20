using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

/// Single-row runtime-configurable settings table — there is exactly one row, read/written via
/// IAppSettingsService, seeded with a fixed Id in AppDbContext.OnModelCreating.
public class AppSettings
{
    public Guid Id { get; set; }
    public ImageGenerationProvider ImageGenerationProvider { get; set; } = ImageGenerationProvider.OpenAI;

    /// The one product's base price (see #392) — every new order gets this value explicitly at
    /// creation time (OrdersController.Create), never recomputed afterward, so changing this here
    /// never retroactively repriced an already-placed order.
    public decimal BasePrice { get; set; } = 1600m;
}
