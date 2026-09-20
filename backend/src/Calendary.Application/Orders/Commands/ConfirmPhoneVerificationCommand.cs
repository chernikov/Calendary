using Calendary.Application.Common;
using Calendary.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Commands;

public record ConfirmPhoneVerificationCommand(Guid UserId, string? Code) : IRequest;

public class ConfirmPhoneVerificationCommandHandler(IAppDbContext db) : IRequestHandler<ConfirmPhoneVerificationCommand>
{
    // Mirrors ConfirmEmailCommand's lockout (#301) — after this many wrong guesses, invalidate the
    // code outright rather than let it keep being brute-forced (only a 4-digit space).
    public const int MaxAttempts = 5;

    public async Task Handle(ConfirmPhoneVerificationCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstAsync(u => u.Id == request.UserId, ct);

        var code = request.Code?.Trim() ?? string.Empty;
        if (user.PhoneVerificationPhone is null
            || user.PhoneVerificationCode is null
            || user.PhoneVerificationCodeExpiresAtUtc is null
            || user.PhoneVerificationCodeExpiresAtUtc < DateTime.UtcNow
            || !string.Equals(user.PhoneVerificationCode, code, StringComparison.Ordinal))
        {
            user.PhoneVerificationAttempts++;
            var lockedOut = user.PhoneVerificationAttempts >= MaxAttempts;
            if (lockedOut)
            {
                user.PhoneVerificationPhone = null;
                user.PhoneVerificationCode = null;
                user.PhoneVerificationCodeExpiresAtUtc = null;
            }
            await db.SaveChangesAsync(ct);

            throw new AppOperationException(
                lockedOut ? "Забагато невдалих спроб. Запросіть новий код." : "Невірний код.");
        }

        user.PhoneVerifiedPhone = user.PhoneVerificationPhone;
        user.PhoneVerificationCode = null;
        user.PhoneVerificationCodeExpiresAtUtc = null;
        user.PhoneVerificationAttempts = 0;
        await db.SaveChangesAsync(ct);
    }
}
