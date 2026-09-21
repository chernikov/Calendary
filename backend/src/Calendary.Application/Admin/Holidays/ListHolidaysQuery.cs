using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.Holidays;

public record ListHolidaysQuery : IRequest<IReadOnlyList<Holiday>>;

public class ListHolidaysQueryHandler(IAppDbContext db) : IRequestHandler<ListHolidaysQuery, IReadOnlyList<Holiday>>
{
    public async Task<IReadOnlyList<Holiday>> Handle(ListHolidaysQuery request, CancellationToken ct) =>
        await db.Holidays
            .AsNoTracking()
            .OrderBy(h => h.Country).ThenBy(h => h.Year).ThenBy(h => h.Month).ThenBy(h => h.Day)
            .ToListAsync(ct);
}
