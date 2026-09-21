using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record RemovePersonalDateCommand(Guid UserId, Guid OrderId, Guid DateId) : IRequest<Order?>;

public class RemovePersonalDateCommandHandler(IAppDbContext db) : IRequestHandler<RemovePersonalDateCommand, Order?>
{
    public async Task<Order?> Handle(RemovePersonalDateCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;

        var date = order.PersonalDates.FirstOrDefault(d => d.Id == request.DateId);
        if (date is null) return null;

        db.PersonalDates.Remove(date);
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
