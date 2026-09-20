using Calendary.Common;
using Calendary.Domain.Abstractions;
using MediatR;

namespace Calendary.Application.Auth;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

public class ResetPasswordCommandHandler(IPasswordAuthService passwordAuth, ISessionTokenService sessionTokens)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            throw new AppOperationException("Посилання недійсне або застаріло.");
        }
        if (request.NewPassword.Length < PasswordPolicy.MinLength)
        {
            throw new AppOperationException($"Пароль має містити щонайменше {PasswordPolicy.MinLength} символів.");
        }

        var user = await passwordAuth.ResetPasswordAsync(request.Token, request.NewPassword, ct);
        if (user is null)
        {
            throw new AppOperationException("Посилання недійсне або застаріло.");
        }

        // A stolen bearer token (or a stale session on another device) stops working the moment
        // the legitimate owner regains control via reset — same reasoning as forcing a re-login
        // after any credential change.
        await sessionTokens.InvalidateAllSessionsAsync(user.Id, ct);
    }
}
