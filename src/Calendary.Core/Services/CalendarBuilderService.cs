using System.Globalization;
using Calendary.Core.Models;
using Calendary.Model;
using Calendary.Repos.Repositories;
using CalendarEntity = Calendary.Model.Calendar;

namespace Calendary.Core.Services;

public interface ICalendarBuilderService
{
    Task<CalendarEntity> BuildCalendar2026Async(
        Dictionary<int, string> monthlyImages,
        string language,
        DayOfWeek startDay);

    CalendarMonthGrid GenerateMonthGrid(int year, int month);

    Task<IEnumerable<Holiday>> ApplyHolidaysAsync(CalendarGrid grid, int year, string country);

    void AssignImages(CalendarGrid grid, Dictionary<int, string> monthlyImages);

    void Validate(Dictionary<int, string> monthlyImages, string language);

    CalendarGrid? LastGeneratedGrid { get; }
}

public class CalendarBuilderService(IHolidayRepository holidayRepository) : ICalendarBuilderService
{
    private const int TargetYear = 2026;
    private DayOfWeek _currentStartDay = DayOfWeek.Monday;
    private string _currentLanguage = "uk-UA";

    public CalendarGrid? LastGeneratedGrid { get; private set; }

    public async Task<CalendarEntity> BuildCalendar2026Async(
        Dictionary<int, string> monthlyImages,
        string language,
        DayOfWeek startDay)
    {
        Validate(monthlyImages, language);

        _currentLanguage = NormalizeLanguage(language);
        _currentStartDay = startDay;

        var monthNames = GetMonthNames(_currentLanguage);
        var dayNames = GetDayNames(_currentLanguage, startDay);

        var months = Enumerable.Range(1, 12)
            .Select(month => GenerateMonthGrid(TargetYear, month))
            .ToList();

        var grid = new CalendarGrid
        {
            Year = TargetYear,
            Language = _currentLanguage,
            StartDay = startDay,
            MonthNames = monthNames,
            DayNames = dayNames,
            Months = months
        };

        var holidays = await ApplyHolidaysAsync(grid, TargetYear, Country.Ukraine.Code);
        AssignImages(grid, monthlyImages);
        LastGeneratedGrid = grid;

        return BuildCalendar(monthlyImages, holidays, startDay, _currentLanguage);
    }

    public CalendarMonthGrid GenerateMonthGrid(int year, int month)
    {
        var firstDayOfMonth = new DateTime(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var leadingEmptyCells = GetLeadingEmptyCells(firstDayOfMonth.DayOfWeek, _currentStartDay);
        var totalCells = leadingEmptyCells + daysInMonth;
        var trailingEmptyCells = (7 - totalCells % 7) % 7;
        var totalWeeks = (totalCells + trailingEmptyCells) / 7;

        var previousMonthDate = firstDayOfMonth.AddMonths(-1);
        var previousMonthDays = DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month);
        var previousMonthStart = previousMonthDays - leadingEmptyCells + 1;

        var nextMonthDate = firstDayOfMonth.AddMonths(1);
        var currentDay = 1;
        var nextMonthDay = 1;

        var weeks = new List<IReadOnlyList<CalendarDayCell>>(totalWeeks);

        for (var weekIndex = 0; weekIndex < totalWeeks; weekIndex++)
        {
            var weekCells = new List<CalendarDayCell>(7);
            for (var dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                if (leadingEmptyCells > 0)
                {
                    var date = new DateTime(previousMonthDate.Year, previousMonthDate.Month, previousMonthStart++);
                    weekCells.Add(CreateDayCell(date, false));
                    leadingEmptyCells--;
                    continue;
                }

                if (currentDay <= daysInMonth)
                {
                    var date = new DateTime(year, month, currentDay++);
                    weekCells.Add(CreateDayCell(date, true));
                }
                else
                {
                    var date = new DateTime(nextMonthDate.Year, nextMonthDate.Month, nextMonthDay++);
                    weekCells.Add(CreateDayCell(date, false));
                }
            }

            weeks.Add(weekCells);
        }

        return new CalendarMonthGrid
        {
            Month = month,
            Name = GetMonthNames(_currentLanguage)[month - 1],
            Weeks = weeks
        };
    }

    public async Task<IEnumerable<Holiday>> ApplyHolidaysAsync(CalendarGrid grid, int year, string country)
    {
        var countryId = ResolveCountryId(country);
        var holidays = await holidayRepository.GetAllByCoutryIdAsync(countryId);
        var holidaysForYear = holidays.Where(h => h.Date.Year == year).ToArray();

        foreach (var holiday in holidaysForYear)
        {
            var targetMonth = grid.Months.FirstOrDefault(m => m.Month == holiday.Date.Month);
            if (targetMonth is null)
            {
                continue;
            }

            var targetDay = targetMonth.Weeks
                .SelectMany(w => w)
                .FirstOrDefault(d => d.IsCurrentMonth &&
                                     d.Date.Month == holiday.Date.Month &&
                                     d.Date.Day == holiday.Date.Day);

            if (targetDay is not null)
            {
                targetDay.IsHoliday = true;
                targetDay.HolidayName = holiday.Name;
            }
        }

        return holidaysForYear;
    }

    public void AssignImages(CalendarGrid grid, Dictionary<int, string> monthlyImages)
    {
        foreach (var month in grid.Months)
        {
            if (monthlyImages.TryGetValue(month.Month, out var image))
            {
                month.ImageUrl = image;
            }
        }
    }

    public void Validate(Dictionary<int, string> monthlyImages, string language)
    {
        if (monthlyImages is null)
        {
            throw new ArgumentNullException(nameof(monthlyImages));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            throw new ArgumentException("Language is required", nameof(language));
        }

        if (monthlyImages.Count != 12)
        {
            throw new ArgumentException("Images for all 12 months are required", nameof(monthlyImages));
        }

        var invalidMonths = monthlyImages.Keys.Where(k => k is < 1 or > 12).ToArray();
        if (invalidMonths.Length > 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyImages), "Month keys must be between 1 and 12.");
        }
    }

    private CalendarEntity BuildCalendar(
        Dictionary<int, string> monthlyImages,
        IEnumerable<Holiday> holidays,
        DayOfWeek startDay,
        string language)
    {
        var resolvedLanguage = ResolveLanguage(language);

        var calendar = new CalendarEntity
        {
            Year = TargetYear,
            FirstDayOfWeek = startDay,
            LanguageId = resolvedLanguage.Id,
            Language = resolvedLanguage,
            CountryId = Country.Ukraine.Id,
            Country = Country.Ukraine
        };

        calendar.Images = monthlyImages
            .OrderBy(pair => pair.Key)
            .Select(pair => new Image
            {
                Calendar = calendar,
                CalendarId = calendar.Id,
                MonthNumber = (short)pair.Key,
                ImageUrl = pair.Value
            })
            .ToList();

        calendar.CalendarHolidays = holidays
            .Select(h => new CalendarHoliday
            {
                Calendar = calendar,
                CalendarId = calendar.Id,
                Holiday = h,
                HolidayId = h.Id
            })
            .ToList();

        return calendar;
    }

    private static string NormalizeLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "uk-UA";
        }

        try
        {
            return CultureInfo.GetCultureInfo(language).Name;
        }
        catch (CultureNotFoundException)
        {
            return "uk-UA";
        }
    }

    private static Language ResolveLanguage(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "uk" or "uk-ua" => Language.Ukrainian,
            "ru" or "ru-ru" => Language.Ukrainian, // fallback to Ukrainian content
            "en" or "en-us" or "en-en" or "en-gb" => Language.English,
            _ => Language.Ukrainian
        };
    }

    private static int ResolveCountryId(string countryCode)
    {
        if (string.Equals(countryCode, Country.Ukraine.Code, StringComparison.OrdinalIgnoreCase))
        {
            return Country.Ukraine.Id;
        }

        return Country.Ukraine.Id;
    }

    private static string[] GetMonthNames(string languageCode)
    {
        var culture = new CultureInfo(languageCode);
        return culture.DateTimeFormat.MonthNames[..12];
    }

    private static string[] GetDayNames(string languageCode, DayOfWeek startDay)
    {
        var culture = new CultureInfo(languageCode);
        var dayNames = culture.DateTimeFormat.AbbreviatedDayNames;

        string[] reorderedDayNames = new string[7];
        int startIndex = (int)startDay;

        for (int i = 0; i < 7; i++)
        {
            reorderedDayNames[i] = dayNames[(startIndex + i) % 7];
        }

        return reorderedDayNames;
    }

    private static int GetLeadingEmptyCells(DayOfWeek firstDayOfMonth, DayOfWeek startDay)
    {
        return ((int)firstDayOfMonth - (int)startDay + 7) % 7;
    }

    private static CalendarDayCell CreateDayCell(DateTime date, bool isCurrentMonth)
    {
        return new CalendarDayCell
        {
            Date = date,
            IsCurrentMonth = isCurrentMonth,
            IsWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
        };
    }
}
