using Calendary.Application.Common;
using Calendary.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.Prompts;

public record DeletePromptCommand(Guid Id) : IRequest<bool>;

public class DeletePromptCommandHandler(IAppDbContext db) : IRequestHandler<DeletePromptCommand, bool>
{
    public async Task<bool> Handle(DeletePromptCommand request, CancellationToken ct)
    {
        var prompt = await db.Prompts.FindAsync([request.Id], ct);
        if (prompt is null) return false;
        if (await db.Sheets.AnyAsync(s => s.PromptId == request.Id, ct))
        {
            throw new AppOperationException("Prompt is used by existing orders.", 409);
        }

        db.Prompts.Remove(prompt);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
