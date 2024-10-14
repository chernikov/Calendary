namespace Calendary.Api.Dtos;

public class CalendarDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public DayOfWeek FirstDayOfWeek { get; set; }
    public int LanguageId { get; set; }
    public int OrderId { get; set; }
}
