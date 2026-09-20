using Calendary.Application.Common;
using Calendary.Application.Orders;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Admin.Orders;

public record AdminGetOrderQuery(Guid OrderId) : IRequest<Order?>;

public class AdminGetOrderQueryHandler(IAppDbContext db) : IRequestHandler<AdminGetOrderQuery, Order?>
{
    public Task<Order?> Handle(AdminGetOrderQuery request, CancellationToken ct) =>
        OrderAccess.LoadOrderForAdminAsync(db, request.OrderId, ct);
}
