using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Holidays;

// Customer-facing equivalent of Admin.Holidays.ListHolidaysQuery — filtered to one year and
// unauthenticated (the style-dates step fetches this once, see #364/#366).
public record ListHolidaysForYearQuery(int Year) : IRequest<IReadOnlyList<Holiday>>;

public class ListHolidaysForYearQueryHandler(IAppDbContext db) : IRequestHandler<ListHolidaysForYearQuery, IReadOnlyList<Holiday>>
{
    public async Task<IReadOnlyList<Holiday>> Handle(ListHolidaysForYearQuery request, CancellationToken ct) =>
        await db.Holidays
            .Where(h => h.Year == request.Year)
            .OrderBy(h => h.Month).ThenBy(h => h.Day)
            .ToListAsync(ct);
}
