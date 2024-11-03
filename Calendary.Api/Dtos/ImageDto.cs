namespace Calendary.Api.Dtos;

public class ImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public short MonthNumber { get; set; }
}
