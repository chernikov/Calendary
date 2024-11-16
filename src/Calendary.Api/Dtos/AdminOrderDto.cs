namespace Calendary.Api.Dtos;

public class AdminOrderDto
{
    public int Id { get; set; }

    public UserInfoDto User { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsPaid { get; set; }

    public string? Comment { get; set; }

    public string? DeliveryAddress { get; set; }

    public ICollection<OrderItemDto> Items { get; set; } = [];
}
