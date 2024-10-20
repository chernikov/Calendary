﻿namespace Calendary.Model;

public class Calendar
{
    public int Id { get; set; }

    public bool IsCurrent { get; set; }

    public int Year { get; set; }

    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public ICollection<Image> Images { get; set; } = [];
    public ICollection<EventDate> EventDates { get; set; } = [];
    public ICollection<CalendarHoliday> CalendarHolidays { get; set; } = [];

    public string? FilePath { get; set; }
}
