using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.ImageStyles;

public record ListImageStylesQuery : IRequest<IReadOnlyList<ImageStyle>>;

public class ListImageStylesQueryHandler(IAppDbContext db) : IRequestHandler<ListImageStylesQuery, IReadOnlyList<ImageStyle>>
{
    public async Task<IReadOnlyList<ImageStyle>> Handle(ListImageStylesQuery request, CancellationToken ct) =>
        await db.ImageStyles.OrderBy(s => s.SortOrder).ToListAsync(ct);
}
