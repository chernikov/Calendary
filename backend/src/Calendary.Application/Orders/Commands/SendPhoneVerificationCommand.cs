using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Commands;

// Scoped to the user, not an order (#304) — works uniformly for both single-order checkout and
// batch checkout, which share one delivery phone across several orders.
public record SendPhoneVerificationCommand(Guid UserId, string Phone) : IRequest;

public class SendPhoneVerificationCommandHandler(IAppDbContext db, ISmsService sms) : IRequestHandler<SendPhoneVerificationCommand>
{
    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    public async Task Handle(SendPhoneVerificationCommand request, CancellationToken ct)
    {
        var phone = UkrainianPhoneNumber.Normalize(request.Phone);
        if (phone is null)
        {
            throw new AppOperationException("Невірний формат телефону. Приклад: +380671234567.");
        }

        var user = await db.Users.FirstAsync(u => u.Id == request.UserId, ct);
        var code = await sms.SendVerificationCodeAsync(phone, ct);

        user.PhoneVerificationPhone = phone;
        user.PhoneVerificationCode = code;
        user.PhoneVerificationCodeExpiresAtUtc = DateTime.UtcNow.Add(CodeLifetime);
        user.PhoneVerificationAttempts = 0;
        // A fresh code (possibly for a different number) invalidates any prior verification.
        user.PhoneVerifiedPhone = null;
        await db.SaveChangesAsync(ct);
    }
}
