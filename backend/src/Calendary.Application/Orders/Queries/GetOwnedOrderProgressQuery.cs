using Calendary.Application.Common;
using MediatR;

namespace Calendary.Application.Orders.Queries;

public record GetOwnedOrderProgressQuery(Guid UserId, Guid OrderId) : IRequest<OrderProgress?>;

public class GetOwnedOrderProgressQueryHandler(IAppDbContext db) : IRequestHandler<GetOwnedOrderProgressQuery, OrderProgress?>
{
    public Task<OrderProgress?> Handle(GetOwnedOrderProgressQuery request, CancellationToken ct) =>
        OrderAccess.LoadOwnedOrderProgressAsync(db, request.UserId, request.OrderId, ct);
}
