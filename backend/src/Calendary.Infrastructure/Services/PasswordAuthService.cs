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
            AuthProvider = AuthProvider.Password
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
}
