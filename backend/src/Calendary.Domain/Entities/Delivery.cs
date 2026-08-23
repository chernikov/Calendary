namespace Calendary.Domain.Entities;

public class Delivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public string RecipientName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string City { get; set; } = default!;
    public string WarehouseNumber { get; set; } = default!;
    public string WarehouseAddress { get; set; } = default!;
    public string? TrackingNumber { get; set; }
}
