using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record RemovePromoCodeCommand(Guid UserId, Guid OrderId) : IRequest<Order?>;

public class RemovePromoCodeCommandHandler(IAppDbContext db) : IRequestHandler<RemovePromoCodeCommand, Order?>
{
    public async Task<Order?> Handle(RemovePromoCodeCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;

        order.PromoCode = null;
        order.DiscountAmount = 0m;
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
