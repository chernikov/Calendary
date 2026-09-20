using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.PromptThemes;

// Id null = create, Id set = update — the frontend's admin.service.ts already treats these as one
// "save" call (POST vs PUT based on payload.id), so merging them here removes duplicate
// validate→assign→save logic instead of keeping separate Create/Update commands.
public record SavePromptThemeCommand(Guid? Id, string Name, string Description, int SortOrder) : IRequest<PromptTheme?>;

public class SavePromptThemeCommandHandler(IAppDbContext db) : IRequestHandler<SavePromptThemeCommand, PromptTheme?>
{
    public async Task<PromptTheme?> Handle(SavePromptThemeCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new AppOperationException("Name is required.");

        PromptTheme theme;
        if (request.Id is null)
        {
            theme = new PromptTheme();
            db.PromptThemes.Add(theme);
        }
        else
        {
            // Include(Prompts) — the returned theme is mapped to a DTO that lists its prompts.
            var existing = await db.PromptThemes.Include(t => t.Prompts).FirstOrDefaultAsync(t => t.Id == request.Id.Value, ct);
            if (existing is null) return null;
            theme = existing;
        }

        theme.Name = request.Name.Trim();
        theme.Description = request.Description?.Trim() ?? "";
        theme.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);
        return theme;
    }
}
