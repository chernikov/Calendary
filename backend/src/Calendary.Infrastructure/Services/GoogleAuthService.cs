using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Options;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

public class GoogleAuthService(AppDbContext db, IOptions<GoogleOptions> options) : IGoogleAuthService
{
    public async Task<User> AuthenticateAsync(string idToken, CancellationToken ct = default)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [options.Value.ClientId]
            });
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidGoogleTokenException(ex.Message);
        }

        var email = payload.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is not null)
        {
            // Matching by verified email — signing in with Google on an email that already has a
            // password account just logs into that same account rather than creating a duplicate.
            // Google already verified ownership of this address, so it settles any pending
            // confirmation from the password-registration flow too.
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                user.EmailConfirmationCode = null;
                user.EmailConfirmationCodeExpiresAtUtc = null;
                await db.SaveChangesAsync(ct);
            }
            return user;
        }

        user = new User
        {
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(payload.Name) ? email.Split('@')[0] : payload.Name,
            AuthProvider = AuthProvider.Google,
            EmailConfirmed = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }
}
