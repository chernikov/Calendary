using Calendary.Domain.Enums;

namespace Calendary.Domain.Abstractions;

public record ChargeResult(bool Succeeded, string? FailureReason);

public interface IPaymentService
{
    Task<ChargeResult> ChargeAsync(Guid orderId, PaymentMethod method, decimal amount, CancellationToken ct = default);
}
