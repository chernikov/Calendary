using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.PromoCodes;

public record ListPromoCodesQuery : IRequest<IReadOnlyList<PromoCode>>;

public class ListPromoCodesQueryHandler(IAppDbContext db) : IRequestHandler<ListPromoCodesQuery, IReadOnlyList<PromoCode>>
{
    public async Task<IReadOnlyList<PromoCode>> Handle(ListPromoCodesQuery request, CancellationToken ct) =>
        await db.PromoCodes.OrderByDescending(p => p.IsActive).ThenBy(p => p.Code).ToListAsync(ct);
}
