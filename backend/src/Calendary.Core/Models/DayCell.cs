namespace Calendary.Core.Models;

public class DayCell
{
    public DateTime Date { get; init; }

    public bool IsCurrentMonth { get; init; }

    public bool IsWeekend { get; init; }

    public bool IsToday { get; init; }
}
