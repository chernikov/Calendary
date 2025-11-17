namespace Calendary.Core.Models;

public class MonthGrid
{
    public int Year { get; init; }

    public int Month { get; init; }

    public string MonthName { get; init; } = string.Empty;

    public DayOfWeek StartDay { get; init; }

    public IReadOnlyList<IReadOnlyList<DayCell>> Weeks { get; init; } = Array.Empty<IReadOnlyList<DayCell>>();
}
