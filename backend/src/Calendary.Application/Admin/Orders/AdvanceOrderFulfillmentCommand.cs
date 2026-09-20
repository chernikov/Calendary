using Calendary.Application.Common;
using Calendary.Application.Orders;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Admin.Orders;

// Fulfillment (Paid -> Printing -> PrintReady -> Shipped -> Delivered) is admin-driven, one step
// at a time (#434) — there's no real printer/courier integration to trigger it automatically, so
// FulfillmentBackgroundService's timer simulation was replaced outright rather than kept as a
// fallback alongside manual control.
public record AdvanceOrderFulfillmentCommand(Guid OrderId) : IRequest<Order?>;

public class AdvanceOrderFulfillmentCommandHandler(IAppDbContext db) : IRequestHandler<AdvanceOrderFulfillmentCommand, Order?>
{
    // Real Nova Poshta tracking numbers are tracked separately (#432) — this fake generation is
    // carried over verbatim from the old FulfillmentBackgroundService.
    private static readonly Dictionary<OrderStatus, OrderStatus> NextStatus = new()
    {
        [OrderStatus.Paid] = OrderStatus.Printing,
        [OrderStatus.Printing] = OrderStatus.PrintReady,
        [OrderStatus.PrintReady] = OrderStatus.Shipped,
        [OrderStatus.Shipped] = OrderStatus.Delivered,
    };

    public async Task<Order?> Handle(AdvanceOrderFulfillmentCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOrderForAdminAsync(db, request.OrderId, ct);
        if (order is null) return null;

        if (!NextStatus.TryGetValue(order.Status, out var next))
        {
            throw new AppOperationException($"Замовлення в статусі {order.Status} не має наступного кроку виконання.", 409);
        }

        if (next == OrderStatus.Shipped && order.Delivery is not null && order.Delivery.TrackingNumber is null)
        {
            order.Delivery.TrackingNumber = $"2040{Random.Shared.Next(1000000, 9999999)}";
        }

        order.SetStatus(next);
        await db.SaveChangesAsync(ct);
        return order;
    }
}
