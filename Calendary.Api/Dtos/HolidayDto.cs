namespace Calendary.Api.Dtos;

public class HolidayDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } // Наприклад, "Різдво", "День незалежності"

    public int CountryId { get; set; }
}
