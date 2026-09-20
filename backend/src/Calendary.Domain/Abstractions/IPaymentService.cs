namespace Calendary.Domain.Abstractions;

/// Monobank Acquiring (and any future redirect-based provider) is a two-step, asynchronous flow —
/// unlike a synchronous "charge now" call, the caller gets a hosted page to redirect the customer
/// to, and only learns the outcome later via HandleWebhookAsync. When unconfigured (no merchant
/// token), the implementation behind this interface settles the order immediately instead, so
/// local dev needs no real account — same fallback shape as INovaPoshtaService.
public record PaymentInvoice(string PageUrl);

public interface IPaymentService
{
    Task<PaymentInvoice> CreateInvoiceAsync(
        Guid orderId, string redirectUrl, string webHookUrl, CancellationToken ct = default);

    /// Verifies the provider's webhook signature and applies the resulting status to the matching
    /// Payment/Order. Returns false if the signature didn't verify or no matching payment was
    /// found, so the controller can respond with an error status (prompting the provider to retry).
    Task<bool> HandleWebhookAsync(string rawBody, string? signatureHeader, CancellationToken ct = default);
}
