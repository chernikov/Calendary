using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.PromptLibrary;

public record PromptLibraryResult(IReadOnlyList<PromptTheme> Themes, IReadOnlyList<ImageStyle> Styles);

public record GetPromptLibraryQuery : IRequest<PromptLibraryResult>;

public class GetPromptLibraryQueryHandler(IAppDbContext db) : IRequestHandler<GetPromptLibraryQuery, PromptLibraryResult>
{
    public async Task<PromptLibraryResult> Handle(GetPromptLibraryQuery request, CancellationToken ct)
    {
        var themes = await db.PromptThemes
            .AsNoTracking()
            .Include(t => t.Prompts)
            .OrderBy(t => t.SortOrder)
            .ToListAsync(ct);
        var styles = await db.ImageStyles.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync(ct);

        return new PromptLibraryResult(themes, styles);
    }
}
