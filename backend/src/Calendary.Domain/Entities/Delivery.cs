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

    // Nova Poshta Ref GUIDs for the matched city/warehouse (#432) — captured at checkout
    // (OrderAccess.ValidateAndNormalizeDeliveryAsync) since InternetDocument/save needs Refs, not
    // display names. Nullable because deliveries created before #432 shipped have neither.
    public Guid? CityRef { get; set; }
    public Guid? WarehouseRef { get; set; }
}
