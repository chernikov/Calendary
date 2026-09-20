using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record CancelOrderCommand(Guid UserId, Guid OrderId) : IRequest<Order?>;

public class CancelOrderCommandHandler(IAppDbContext db) : IRequestHandler<CancelOrderCommand, Order?>
{
    public async Task<Order?> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (order.Status is OrderStatus.Paid or OrderStatus.Printing or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            throw new AppOperationException("Order has already been paid; cancellation would require a refund flow that is out of scope for this demo.", 409);
        }

        order.SetStatus(OrderStatus.Cancelled);
        await db.SaveChangesAsync(ct);
        return order;
    }
}
