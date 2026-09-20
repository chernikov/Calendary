using Calendary.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Calendary.Application.Auth;

// Always "succeeds" from the caller's perspective — the controller returns the same 200 whether
// or not the email is registered, so this can't be used to enumerate accounts (#301).
public record ForgotPasswordCommand(string Email, string PublicBaseUrl) : IRequest;

public class ForgotPasswordCommandHandler(
    IPasswordAuthService passwordAuth, IEmailService email, ILogger<ForgotPasswordCommandHandler> logger)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var token = await passwordAuth.GeneratePasswordResetTokenAsync(request.Email, ct);
        if (token is null)
        {
            // No password-auth account for this email — silently no-op, same as a real send.
            return;
        }

        var link = $"{request.PublicBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

        try
        {
            await email.SendAsync(
                request.Email.Trim(),
                "Відновлення пароля Calendary",
                "<p>Ви (або хтось інший) запросили відновлення пароля для цього акаунта.</p>" +
                $"<p><a href=\"{link}\">Встановити новий пароль</a></p>" +
                "<p>Посилання дійсне 1 годину. Якщо це були не ви — просто проігноруйте цей лист.</p>",
                ct);
        }
        catch (Exception ex)
        {
            // A failed reset email should never fail the request itself (same reasoning as the
            // welcome/confirmation emails elsewhere in AuthController).
            logger.LogWarning(ex, "Failed to send password reset email to {Email}", request.Email);
        }
    }
}
