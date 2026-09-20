using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Commands;

public record CheckoutCommand(Guid UserId, Guid OrderId, DeliveryInfo Delivery) : IRequest<Order?>;

public class CheckoutCommandHandler(IAppDbContext db, INovaPoshtaService novaPoshta) : IRequestHandler<CheckoutCommand, Order?>
{
    public async Task<Order?> Handle(CheckoutCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);

        var delivery = await OrderAccess.ValidateAndNormalizeDeliveryAsync(novaPoshta, request.Delivery, ct);

        if (order.Delivery is null)
        {
            order.Delivery = new Delivery { OrderId = order.Id };
            db.Deliveries.Add(order.Delivery);
        }
        order.Delivery.RecipientName = delivery.RecipientName;
        order.Delivery.Phone = delivery.Phone;
        order.Delivery.City = delivery.City;
        order.Delivery.WarehouseNumber = delivery.WarehouseNumber;
        order.Delivery.WarehouseAddress = delivery.WarehouseAddress;

        var user = await db.Users.FirstAsync(u => u.Id == request.UserId, ct);
        OrderAccess.RequirePhoneVerified(user, delivery.Phone);
        OrderAccess.RememberLastDelivery(user, delivery);

        order.SetStatus(OrderStatus.AwaitingPayment);
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
