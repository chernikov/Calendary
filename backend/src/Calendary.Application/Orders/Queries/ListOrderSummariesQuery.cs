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
        return await db.Orders
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderSummary(
                o.Id,
                o.Status.ToString(),
                o.Price,
                o.PrintQuantity,
                o.CreatedAtUtc,
                o.StatusUpdatedAtUtc,
                o.Sheets.Where(s => s.Kind == SheetKind.Cover && s.Prompt != null).Select(s => s.Prompt!.Name).FirstOrDefault(),
                o.Sheets.Where(s => s.Kind == SheetKind.Cover).Select(s => s.ImageUrl).FirstOrDefault(),
                o.IsArchived))
            .ToListAsync(ct);
    }
}
