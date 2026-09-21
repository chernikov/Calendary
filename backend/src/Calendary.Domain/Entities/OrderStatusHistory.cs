using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

// Persisted alongside DomainStatusLoggingInterceptor's existing log line (#434) — one row per
// Order.Status transition, including the initial Created state (FromStatus null).
public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
