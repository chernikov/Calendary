using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record CheckoutCommand(Guid UserId, Guid OrderId, DeliveryInfo Delivery) : IRequest<Order?>;

public class CheckoutCommandHandler(IAppDbContext db) : IRequestHandler<CheckoutCommand, Order?>
{
    public async Task<Order?> Handle(CheckoutCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);

        if (order.Delivery is null)
        {
            order.Delivery = new Delivery { OrderId = order.Id };
            db.Deliveries.Add(order.Delivery);
        }
        order.Delivery.RecipientName = request.Delivery.RecipientName;
        order.Delivery.Phone = request.Delivery.Phone;
        order.Delivery.City = request.Delivery.City;
        order.Delivery.WarehouseNumber = request.Delivery.WarehouseNumber;
        order.Delivery.WarehouseAddress = request.Delivery.WarehouseAddress;

        order.SetStatus(OrderStatus.AwaitingPayment);
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
