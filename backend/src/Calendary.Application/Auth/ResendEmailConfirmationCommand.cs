using Calendary.Application.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Calendary.Application.Auth;

public record ResendEmailConfirmationCommand(Guid UserId) : IRequest<User?>;

public class ResendEmailConfirmationCommandHandler(
    IAppDbContext db, IEmailService email, ILogger<ResendEmailConfirmationCommandHandler> logger)
    : IRequestHandler<ResendEmailConfirmationCommand, User?>
{
    public async Task<User?> Handle(ResendEmailConfirmationCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null) return null;
        if (user.EmailConfirmed) return user;

        user.EmailConfirmationCode = EmailConfirmationCodeGenerator.Generate();
        user.EmailConfirmationCodeExpiresAtUtc = DateTime.UtcNow.Add(EmailConfirmationCodeGenerator.Lifetime);
        user.EmailConfirmationAttempts = 0;
        await db.SaveChangesAsync(ct);

        try
        {
            await email.SendAsync(
                user.Email!,
                "Код підтвердження Calendary",
                "<p>Ваш код підтвердження пошти:</p>" +
                $"<p><strong style=\"font-size:20px;letter-spacing:4px;\">{user.EmailConfirmationCode}</strong></p>" +
                "<p>Код дійсний 30 хвилин.</p>",
                ct);
        }
        catch (Exception ex)
        {
            // A failed resend email should never fail the request itself.
            logger.LogWarning(ex, "Failed to send confirmation email to {Email}", user.Email);
        }

        return user;
    }
}
