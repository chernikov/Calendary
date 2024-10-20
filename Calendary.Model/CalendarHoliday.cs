namespace Calendary.Model;

public class CalendarHoliday
{
    public int CalendarId { get; set; }
    public Calendar Calendar { get; set; } = null!;

    public int HolidayId { get; set; }
    public Holiday Holiday { get; set; } = null!;
}
