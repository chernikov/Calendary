using Calendary.Application.Common;
using Calendary.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.PromptThemes;

public record DeletePromptThemeCommand(Guid Id) : IRequest<bool>;

public class DeletePromptThemeCommandHandler(IAppDbContext db) : IRequestHandler<DeletePromptThemeCommand, bool>
{
    public async Task<bool> Handle(DeletePromptThemeCommand request, CancellationToken ct)
    {
        var theme = await db.PromptThemes.Include(t => t.Prompts).FirstOrDefaultAsync(t => t.Id == request.Id, ct);
        if (theme is null) return false;

        var promptIds = theme.Prompts.Select(p => p.Id).ToList();
        if (await db.Sheets.AnyAsync(s => s.PromptId != null && promptIds.Contains(s.PromptId.Value), ct))
        {
            throw new AppOperationException("Theme contains prompts that are used by existing orders.", 409);
        }

        db.PromptThemes.Remove(theme);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
