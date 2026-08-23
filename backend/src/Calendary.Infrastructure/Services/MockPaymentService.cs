using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;

namespace Calendary.Infrastructure.Services;

/// Mock payment provider: always succeeds after a short simulated delay. No real card data,
/// wallet, or bank integration — a real integration would sit behind this same interface.
public class MockPaymentService : IPaymentService
{
    public async Task<ChargeResult> ChargeAsync(Guid orderId, PaymentMethod method, decimal amount, CancellationToken ct = default)
    {
        await Task.Delay(600, ct);
        return new ChargeResult(true, null);
    }
}
