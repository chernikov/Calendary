using Calendary.Application.Common;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Queries;

// The "light" load #298 asks for — a DB-side projection avoiding the full Include-graph
// LoadOwnedOrderAsync pulls, since a list view only needs a handful of scalar fields per order.
public record ListOrderSummariesQuery(Guid UserId) : IRequest<IReadOnlyList<OrderSummary>>;

public class ListOrderSummariesQueryHandler(IAppDbContext db) : IRequestHandler<ListOrderSummariesQuery, IReadOnlyList<OrderSummary>>
{
    public async Task<IReadOnlyList<OrderSummary>> Handle(ListOrderSummariesQuery request, CancellationToken ct)
    {
        // A single LEFT JOIN against the (at most one) cover sheet per order, instead of #303's
        // original two correlated subqueries (cover prompt name + cover ImageUrl) per row — keeps
        // this to one SQL query regardless of list size.
        var query =
            from o in db.Orders.AsNoTracking()
            where o.UserId == request.UserId
            join s in db.Sheets.AsNoTracking() on new { OrderId = o.Id, Kind = SheetKind.Cover }
                equals new { s.OrderId, s.Kind } into coverSheets
            from cover in coverSheets.DefaultIfEmpty()
            orderby o.CreatedAtUtc descending
            select new OrderSummary(
                o.Id,
                o.Status.ToString(),
                o.Price,
                o.PrintQuantity,
                o.CreatedAtUtc,
                o.StatusUpdatedAtUtc,
                cover != null && cover.Prompt != null ? cover.Prompt.Name : null,
                cover != null ? cover.ImageUrl : null,
                o.IsArchived);

        return await query.ToListAsync(ct);
    }
}
