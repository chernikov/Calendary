using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record SetPrintQuantityCommand(Guid UserId, Guid OrderId, int Quantity) : IRequest<Order?>;

public class SetPrintQuantityCommandHandler(IAppDbContext db) : IRequestHandler<SetPrintQuantityCommand, Order?>
{
    public async Task<Order?> Handle(SetPrintQuantityCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (!OrderAccess.SelectableForCheckout.Contains(order.Status))
        {
            throw new AppOperationException("Print quantity can only be changed before payment.", 409);
        }
        if (request.Quantity is < 1 or > 20)
        {
            throw new AppOperationException("Quantity must be between 1 and 20.");
        }

        order.PrintQuantity = request.Quantity;
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
