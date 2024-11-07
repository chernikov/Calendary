namespace Calendary.Api.Dtos;

public class OrderItemDto
{
    public int Id { get; set; }

    public CalendarDto Calendar { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}
