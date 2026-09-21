using Calendary.Application.Common;
using Calendary.Application.Orders;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Admin.Orders;

// Fulfillment (Paid -> Printing -> PrintReady -> Shipped -> Delivered) is admin-driven, one step
// at a time (#434) — there's no real printer/courier integration to trigger it automatically, so
// FulfillmentBackgroundService's timer simulation was replaced outright rather than kept as a
// fallback alongside manual control.
public record AdvanceOrderFulfillmentCommand(Guid OrderId) : IRequest<Order?>;

public class AdvanceOrderFulfillmentCommandHandler(IAppDbContext db, INovaPoshtaService novaPoshta)
    : IRequestHandler<AdvanceOrderFulfillmentCommand, Order?>
{
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

        // Real Nova Poshta shipment creation (#432) — CityRef/WarehouseRef are only populated for
        // deliveries created after #432 shipped; older orders fall back to no tracking number
        // rather than crashing the transition (nothing to ship a real waybill against).
        if (next == OrderStatus.Shipped && order.Delivery is not null && order.Delivery.TrackingNumber is null
            && order.Delivery.CityRef is { } cityRef && order.Delivery.WarehouseRef is { } warehouseRef)
        {
            var (firstName, lastName) = SplitRecipientName(order.Delivery.RecipientName);
            var shipment = await novaPoshta.CreateShipmentAsync(
                new NovaPoshtaShipmentRecipient(firstName, lastName, order.Delivery.Phone, cityRef, warehouseRef), ct);
            order.Delivery.TrackingNumber = shipment.TrackingNumber;
        }

        order.SetStatus(next);
        await db.SaveChangesAsync(ct);
        return order;
    }

    // RecipientName is one free-text field at checkout ("Отримувач"), but Nova Poshta's
    // Counterparty/save needs FirstName/LastName separately — splits on the first space,
    // matching the "Прізвище Ім'я" order Ukrainian shipping forms conventionally ask for. Doesn't
    // need to be exactly right: Nova Poshta ties the shipment to the phone number, not name order.
    private static (string FirstName, string LastName) SplitRecipientName(string recipientName)
    {
        var parts = recipientName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => ("Клієнт", "Calendary"),
            1 => (parts[0], parts[0]),
            _ => (parts[1], parts[0]),
        };
    }
}
