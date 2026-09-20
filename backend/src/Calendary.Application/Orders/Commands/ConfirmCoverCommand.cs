using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record ConfirmCoverCommand(Guid UserId, Guid OrderId, Guid SheetId) : IRequest<Order?>;

public class ConfirmCoverCommandHandler(IAppDbContext db) : IRequestHandler<ConfirmCoverCommand, Order?>
{
    public async Task<Order?> Handle(ConfirmCoverCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;

        var cover = order.Sheets.FirstOrDefault(s => s.Id == request.SheetId && s.Kind == SheetKind.Cover);
        if (cover is null) throw new AppOperationException("Not a cover sheet.");
        if (cover.Status != SheetStatus.Ready) throw new AppOperationException("Cover is not ready yet.", 409);

        cover.IsSelected = true;
        if (order.Status is OrderStatus.CoverReady or OrderStatus.Generating)
        {
            order.SetStatus(OrderStatus.CoverConfirmed);
        }
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
