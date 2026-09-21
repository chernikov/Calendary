using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Commands;

public record CheckoutBatchCommand(Guid UserId, IReadOnlyList<Guid> OrderIds, DeliveryInfo Delivery) : IRequest;

public class CheckoutBatchCommandHandler(IAppDbContext db, INovaPoshtaService novaPoshta) : IRequestHandler<CheckoutBatchCommand>
{
    public async Task Handle(CheckoutBatchCommand request, CancellationToken ct)
    {
        if (request.OrderIds.Count == 0) throw new AppOperationException("No orders selected.");

        var orders = await db.Orders
            .Include(o => o.Delivery)
            .Where(o => request.OrderIds.Contains(o.Id) && o.UserId == request.UserId)
            .ToListAsync(ct);
        if (orders.Count != request.OrderIds.Count) throw new AppOperationException("One or more orders were not found.", 404);
        if (orders.Any(OrderAccess.IsExpired)) throw new AppOperationException("One or more orders have expired.", 409);
        if (orders.Any(o => !OrderAccess.SelectableForCheckout.Contains(o.Status)))
        {
            throw new AppOperationException("One or more orders are not ready for checkout.", 409);
        }

        var delivery = await OrderAccess.ValidateAndNormalizeDeliveryAsync(novaPoshta, request.Delivery, ct);

        var user = await db.Users.FirstAsync(u => u.Id == request.UserId, ct);
        OrderAccess.RequirePhoneVerified(user, delivery.Phone);

        foreach (var order in orders)
        {
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
            order.SetStatus(OrderStatus.AwaitingPayment);
        }

        OrderAccess.RememberLastDelivery(user, delivery);

        await db.SaveChangesAsync(ct);
    }
}
