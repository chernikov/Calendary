namespace Calendary.Model;

public class Image
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public short MonthNumber { get; set; }

    public int CalendarId { get; set; }
    public Calendar Calendar { get; set; } = null!;
}
