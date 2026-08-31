using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

/// Single-row runtime-configurable settings table — there is exactly one row, read/written via
/// IAppSettingsService, seeded with a fixed Id in AppDbContext.OnModelCreating.
public class AppSettings
{
    public Guid Id { get; set; }
    public ImageGenerationProvider ImageGenerationProvider { get; set; } = ImageGenerationProvider.OpenAI;
}
