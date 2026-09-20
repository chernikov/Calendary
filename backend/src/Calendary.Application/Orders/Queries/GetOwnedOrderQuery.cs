using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Orders.Queries;

public record GetOwnedOrderQuery(Guid UserId, Guid OrderId) : IRequest<Order?>;

public class GetOwnedOrderQueryHandler(IAppDbContext db) : IRequestHandler<GetOwnedOrderQuery, Order?>
{
    public Task<Order?> Handle(GetOwnedOrderQuery request, CancellationToken ct) =>
        OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
}
