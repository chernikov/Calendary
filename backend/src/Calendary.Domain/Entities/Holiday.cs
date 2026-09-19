using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

/// Admin-managed public holiday, one row per concrete date (see #364) — looked up dynamically by
/// (Year, Country) when rendering the calendar PDF, not referenced by any FK, so it can be
/// freely added/edited/deleted from the admin panel without an in-use guard.
public class Holiday
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Country Country { get; set; }
    public int Year { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public string Name { get; set; } = default!;
}
