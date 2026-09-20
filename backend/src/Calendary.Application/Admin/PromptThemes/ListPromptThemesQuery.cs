using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.PromptThemes;

public record ListPromptThemesQuery : IRequest<IReadOnlyList<PromptTheme>>;

public class ListPromptThemesQueryHandler(IAppDbContext db) : IRequestHandler<ListPromptThemesQuery, IReadOnlyList<PromptTheme>>
{
    public async Task<IReadOnlyList<PromptTheme>> Handle(ListPromptThemesQuery request, CancellationToken ct) =>
        await db.PromptThemes.Include(t => t.Prompts).OrderBy(t => t.SortOrder).ToListAsync(ct);
}
