using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; set; }

    /// Monobank's invoiceId — the only handle the webhook payload carries back, so it's how
    /// HandleWebhookAsync finds which Payment/Order to update. Null for the no-merchant-token
    /// local-dev fallback, which never creates a real provider invoice.
    public string? ProviderInvoiceId { get; set; }
}
