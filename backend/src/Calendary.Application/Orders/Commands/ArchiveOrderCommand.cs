using Calendary.Application.Common;
using MediatR;

namespace Calendary.Application.Orders.Commands;

// Archiving is purely a list-visibility flag — orthogonal to the OrderStatus state machine, so
// it's set directly rather than via SetStatus(). Covers both the old Archive and Unarchive
// controller actions (they were identical except for the flag value).
public record ArchiveOrderCommand(Guid UserId, Guid OrderId, bool Archived) : IRequest<bool>;

public class ArchiveOrderCommandHandler(IAppDbContext db) : IRequestHandler<ArchiveOrderCommand, bool>
{
    public async Task<bool> Handle(ArchiveOrderCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return false;

        order.IsArchived = request.Archived;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
