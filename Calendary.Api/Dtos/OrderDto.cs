namespace Calendary.Api.Dtos;

public class OrderDto
{
    public int Id { get; set; }
    public ICollection<OrderItemDto> Items { get; set; } = [];
}
