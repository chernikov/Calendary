using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Auth;

public record ConfirmEmailCommand(Guid UserId, string? Code) : IRequest<User?>;

public class ConfirmEmailCommandHandler(IAppDbContext db) : IRequestHandler<ConfirmEmailCommand, User?>
{
    public async Task<User?> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null) return null;
        if (user.EmailConfirmed) return user;

        var code = request.Code?.Trim() ?? string.Empty;
        if (user.EmailConfirmationCode is null
            || user.EmailConfirmationCodeExpiresAtUtc is null
            || user.EmailConfirmationCodeExpiresAtUtc < DateTime.UtcNow
            || !string.Equals(user.EmailConfirmationCode, code, StringComparison.Ordinal))
        {
            throw new AppOperationException("Невірний або прострочений код.");
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiresAtUtc = null;
        await db.SaveChangesAsync(ct);

        return user;
    }
}
