using System.Security.Cryptography;
using System.Text;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Services;

public class PasswordAuthService(AppDbContext db) : IPasswordAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<User?> RegisterAsync(string email, string password, string? displayName, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
        {
            return null;
        }

        var user = new User
        {
            Email = normalizedEmail,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? normalizedEmail.Split('@')[0] : displayName.Trim(),
            AuthProvider = AuthProvider.Password,
            EmailConfirmationCode = EmailConfirmationCodeGenerator.Generate(),
            EmailConfirmationCodeExpiresAtUtc = DateTime.UtcNow.Add(EmailConfirmationCodeGenerator.Lifetime),
        };
        user.PasswordHash = _hasher.HashPassword(user, password);

        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Lost a race against a concurrent registration with the same email — the unique
            // filtered index on Users.Email caught what the AnyAsync check above couldn't.
            return null;
        }

        return user;
    }

    public async Task<User?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Email == normalizedEmail && u.PasswordHash != null, ct);
        if (user is null)
        {
            return null;
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash!, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Email == normalizedEmail && u.PasswordHash != null, ct);
        if (user is null)
        {
            // No password-auth account for this email (either it doesn't exist, or it's a
            // Google-only account) — the caller must respond identically either way.
            return null;
        }

        var rawToken = GenerateRawToken();
        user.PasswordResetTokenHash = Hash(rawToken);
        user.PasswordResetTokenExpiresAtUtc = DateTime.UtcNow.Add(ResetTokenLifetime);
        await db.SaveChangesAsync(ct);

        return rawToken;
    }

    public async Task<User?> ResetPasswordAsync(string rawToken, string newPassword, CancellationToken ct = default)
    {
        var hash = Hash(rawToken);
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.PasswordResetTokenHash == hash && u.PasswordResetTokenExpiresAtUtc > DateTime.UtcNow, ct);
        if (user is null)
        {
            return null;
        }

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAtUtc = null;
        await db.SaveChangesAsync(ct);

        return user;
    }

    // Same shape as SessionTokenService's raw-token/hash pair — kept as its own small copy rather
    // than a shared helper, since a reset token and a session token are different-lifetime,
    // different-purpose secrets that just happen to use the same generation technique.
    private static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
