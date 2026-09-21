using Calendary.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.Orders;

public record AdminGetOrderStatusHistoryQuery(Guid OrderId) : IRequest<List<AdminOrderStatusHistoryEntry>>;

public class AdminGetOrderStatusHistoryQueryHandler(IAppDbContext db)
    : IRequestHandler<AdminGetOrderStatusHistoryQuery, List<AdminOrderStatusHistoryEntry>>
{
    public Task<List<AdminOrderStatusHistoryEntry>> Handle(AdminGetOrderStatusHistoryQuery request, CancellationToken ct) =>
        db.OrderStatusHistories
            .AsNoTracking()
            .Where(h => h.OrderId == request.OrderId)
            .OrderBy(h => h.ChangedAtUtc)
            .Select(h => new AdminOrderStatusHistoryEntry(
                h.FromStatus == null ? null : h.FromStatus.ToString(), h.ToStatus.ToString(), h.ChangedAtUtc))
            .ToListAsync(ct);
}
