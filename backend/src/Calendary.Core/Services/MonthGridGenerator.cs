using System.Globalization;
using System.Linq;
using Calendary.Core.Models;

namespace Calendary.Core.Services;

public class MonthGridGenerator
{
    private readonly CultureInfo _culture;

    public MonthGridGenerator()
        : this(CultureInfo.InvariantCulture)
    {
    }

    public MonthGridGenerator(CultureInfo culture)
    {
        _culture = culture ?? throw new ArgumentNullException(nameof(culture));
    }

    public MonthGrid Generate(int year, int month, DayOfWeek startDay)
    {
        Validate(year, month);

        var firstDay = new DateTime(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var leadingEmptyCells = CalculateLeadingCells(firstDay.DayOfWeek, startDay);
        var totalCells = leadingEmptyCells + daysInMonth;
        var trailingEmptyCells = (7 - totalCells % 7) % 7;
        var totalWeeks = (totalCells + trailingEmptyCells) / 7;

        var weeks = new List<IReadOnlyList<DayCell>>(totalWeeks);
        var previousMonth = firstDay.AddMonths(-1);
        var previousMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        var previousMonthStart = previousMonthDays - leadingEmptyCells + 1;
        var nextMonth = firstDay.AddMonths(1);
        var nextMonthDay = 1;
        var currentDay = 1;

        for (var weekIndex = 0; weekIndex < totalWeeks; weekIndex++)
        {
            var week = new List<DayCell>(7);
            for (var dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                if (leadingEmptyCells > 0)
                {
                    var date = new DateTime(previousMonth.Year, previousMonth.Month, previousMonthStart++);
                    week.Add(CreateCell(date, false));
                    leadingEmptyCells--;
                    continue;
                }

                if (currentDay <= daysInMonth)
                {
                    var date = new DateTime(year, month, currentDay++);
                    week.Add(CreateCell(date, true));
                    continue;
                }

                var trailingDate = new DateTime(nextMonth.Year, nextMonth.Month, nextMonthDay++);
                week.Add(CreateCell(trailingDate, false));
            }

            weeks.Add(week);
        }

        return new MonthGrid
        {
            Year = year,
            Month = month,
            MonthName = _culture.DateTimeFormat.GetMonthName(month),
            StartDay = startDay,
            Weeks = weeks
        };
    }

    public IReadOnlyList<MonthGrid> GenerateYear(int year, DayOfWeek startDay)
    {
        return Enumerable.Range(1, 12)
            .Select(month => Generate(year, month, startDay))
            .ToList();
    }

    private static DayCell CreateCell(DateTime date, bool isCurrentMonth)
    {
        return new DayCell
        {
            Date = date,
            IsCurrentMonth = isCurrentMonth,
            IsWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
            IsToday = date.Date == DateTime.Today
        };
    }

    private static int CalculateLeadingCells(DayOfWeek firstDay, DayOfWeek startDay)
    {
        return ((int)firstDay - (int)startDay + 7) % 7;
    }

    private static void Validate(int year, int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month));
        }

        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }
    }
}
