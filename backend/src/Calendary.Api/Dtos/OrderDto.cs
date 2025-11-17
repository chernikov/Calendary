namespace Calendary.Api.Dtos;

public class OrderDto
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsPaid { get; set; }

    public string? Comment { get; set; }

    public ICollection<OrderItemDto> Items { get; set; } = [];
}
