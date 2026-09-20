using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.Orders;

public record AdminListOrdersQuery(int Page, int PageSize, string? Status) : IRequest<PagedList<AdminOrderSummary>>;

public class AdminListOrdersQueryHandler(IAppDbContext db) : IRequestHandler<AdminListOrdersQuery, PagedList<AdminOrderSummary>>
{
    public async Task<PagedList<AdminOrderSummary>> Handle(AdminListOrdersQuery request, CancellationToken ct)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Orders.Include(o => o.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var parsed))
        {
            query = query.Where(o => o.Status == parsed);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdminOrderSummary(
                o.Id, o.Status.ToString(), o.UserId, o.User.Email, o.User.DisplayName,
                o.Price, o.Sheets.SelectMany(s => s.Variants).Sum(v => (decimal?)v.CostUsd) ?? 0m,
                o.CreatedAtUtc, o.StatusUpdatedAtUtc))
            .ToListAsync(ct);

        return new PagedList<AdminOrderSummary>(items, total, page, pageSize);
    }
}
