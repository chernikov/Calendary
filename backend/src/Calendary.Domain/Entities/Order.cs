using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public DateTime StatusUpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsArchived { get; set; }

    public decimal Price { get; set; } = 1600m;
    public int RegenerationsRemaining { get; set; } = 10;

    /// Physical print copies of this exact calendar (see #377) — same design/images, printed N
    /// times. Total charged for this order = Price * PrintQuantity. Adjustable from "Мої
    /// замовлення" while the order is still selectable for checkout (ReviewReady/AwaitingPayment);
    /// frozen once paid, same as Price itself.
    public int PrintQuantity { get; set; } = 1;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddHours(48);

    /// Which countries' public holidays to mark in the printed calendar grid (see #364) — customer
    /// picks via a multi-select on the personal-dates step, defaulting to Ukraine.
    public List<Country> HolidayCountries { get; set; } = new() { Country.Ukraine };
    public WeekStartDay WeekStart { get; set; } = WeekStartDay.Monday;

    public ICollection<OrderPhoto> Photos { get; set; } = new List<OrderPhoto>();
    public ICollection<PersonalDate> PersonalDates { get; set; } = new List<PersonalDate>();
    public ICollection<Sheet> Sheets { get; set; } = new List<Sheet>();
    public Payment? Payment { get; set; }
    public Delivery? Delivery { get; set; }

    public void SetStatus(OrderStatus status)
    {
        Status = status;
        StatusUpdatedAtUtc = DateTime.UtcNow;
    }
}
