namespace Calendary.Core.Models;

public class CalendarGrid
{
    public int Year { get; init; }

    public string Language { get; init; } = "uk-UA";

    public DayOfWeek StartDay { get; init; }

    public IReadOnlyList<string> MonthNames { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> DayNames { get; init; } = Array.Empty<string>();

    public IReadOnlyList<CalendarMonthGrid> Months { get; init; } = Array.Empty<CalendarMonthGrid>();
}

public class CalendarMonthGrid
{
    public int Month { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<IReadOnlyList<CalendarDayCell>> Weeks { get; init; } = Array.Empty<IReadOnlyList<CalendarDayCell>>();

    public string? ImageUrl { get; set; }
}

public class CalendarDayCell
{
    public DateTime Date { get; init; }

    public bool IsCurrentMonth { get; init; }

    public bool IsWeekend { get; init; }

    public bool IsHoliday { get; set; }

    public string? HolidayName { get; set; }
}
