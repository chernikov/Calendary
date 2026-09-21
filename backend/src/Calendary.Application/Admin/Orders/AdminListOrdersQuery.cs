using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.Orders;

// Search (#315) matches order ID, customer email/display name, or delivery tracking number — the
// handles a support agent would actually have on hand when a customer gets in touch about a
// specific order.
public record AdminListOrdersQuery(int Page, int PageSize, string? Status, string? Search) : IRequest<PagedList<AdminOrderSummary>>;

public class AdminListOrdersQueryHandler(IAppDbContext db) : IRequestHandler<AdminListOrdersQuery, PagedList<AdminOrderSummary>>
{
    public async Task<PagedList<AdminOrderSummary>> Handle(AdminListOrdersQuery request, CancellationToken ct)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Orders.AsNoTracking().Include(o => o.User).Include(o => o.Delivery).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var parsed))
        {
            query = query.Where(o => o.Status == parsed);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(o =>
                EF.Functions.Like(o.Id.ToString(), $"%{search}%") ||
                (o.User.Email != null && EF.Functions.Like(o.User.Email, $"%{search}%")) ||
                (o.User.DisplayName != null && EF.Functions.Like(o.User.DisplayName, $"%{search}%")) ||
                (o.Delivery != null && o.Delivery.TrackingNumber != null && EF.Functions.Like(o.Delivery.TrackingNumber, $"%{search}%")));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdminOrderSummary(
                o.Id, o.Status.ToString(), o.UserId, o.User.Email, o.User.DisplayName,
                o.Price, o.Sheets.SelectMany(s => s.Variants).Sum(v => (decimal?)v.CostUsd) ?? 0m,
                o.CreatedAtUtc, o.StatusUpdatedAtUtc, o.Delivery != null ? o.Delivery.TrackingNumber : null))
            .ToListAsync(ct);

        return new PagedList<AdminOrderSummary>(items, total, page, pageSize);
    }
}
