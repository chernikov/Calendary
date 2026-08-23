using System.Security.Cryptography;
using System.Text;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Services;

/// DB-backed bearer sessions — deliberately not in-memory, so a container restart (which happens
/// on every deploy) doesn't silently log every user out.
public class SessionTokenService(AppDbContext db) : ISessionTokenService
{
    public async Task<string> IssueTokenAsync(User user, CancellationToken ct = default)
    {
        var rawToken = GenerateRawToken();
        db.UserSessions.Add(new UserSession
        {
            UserId = user.Id,
            TokenHash = Hash(rawToken)
        });
        await db.SaveChangesAsync(ct);
        return rawToken;
    }

    public async Task<User?> ResolveAsync(string bearerToken, CancellationToken ct = default)
    {
        var hash = Hash(bearerToken);
        var session = await db.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.TokenHash == hash, ct);

        return session is null || session.ExpiresAtUtc < DateTime.UtcNow ? null : session.User;
    }

    private static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
