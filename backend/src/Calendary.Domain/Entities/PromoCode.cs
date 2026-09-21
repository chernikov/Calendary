using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

/// A campaign-wide discount code a customer types in at checkout (see #393) — not tied to a
/// specific user/order until OrdersController's apply endpoint freezes its resulting
/// Order.DiscountAmount, so this row itself only tracks the code's own rules and usage counter.
public class PromoCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    /// Always stored/matched uppercase (case-insensitive from the customer's point of view).
    public string Code { get; set; } = default!;
    public DiscountType Type { get; set; }
    /// Percent: 0–100 (e.g. 10 = 10%). FixedAmount: currency units (e.g. 200 = 200₴).
    public decimal Value { get; set; }
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public int? MaxRedemptions { get; set; }
    /// Incremented only on successful payment (webhook/local-dev settle), not on apply — otherwise
    /// abandoned checkouts would burn through the redemption limit for nothing.
    public int RedemptionsUsed { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public bool IsActive { get; set; } = true;
}
