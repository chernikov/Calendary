using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Commands;

public record CheckoutBatchCommand(Guid UserId, IReadOnlyList<Guid> OrderIds, DeliveryInfo Delivery) : IRequest;

public class CheckoutBatchCommandHandler(IAppDbContext db) : IRequestHandler<CheckoutBatchCommand>
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

        foreach (var order in orders)
        {
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
        }
        await db.SaveChangesAsync(ct);
    }
}
