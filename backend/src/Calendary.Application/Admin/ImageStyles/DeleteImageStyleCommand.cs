using Calendary.Application.Common;
using Calendary.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.ImageStyles;

public record DeleteImageStyleCommand(Guid Id) : IRequest<bool>;

public class DeleteImageStyleCommandHandler(IAppDbContext db) : IRequestHandler<DeleteImageStyleCommand, bool>
{
    public async Task<bool> Handle(DeleteImageStyleCommand request, CancellationToken ct)
    {
        var style = await db.ImageStyles.FindAsync([request.Id], ct);
        if (style is null) return false;
        if (await db.Sheets.AnyAsync(s => s.ImageStyleId == request.Id, ct))
        {
            throw new AppOperationException("Style is used by existing orders.", 409);
        }

        db.ImageStyles.Remove(style);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
