using System.Collections.Concurrent;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Services;

/// Passwordless dev auth: no real email/SMS/Google OAuth provider is wired up. "Starting" a
/// login mints a one-time token that a real system would deliver by email/SMS; here it is
/// handed straight back to the caller so the frontend can simulate opening the link.
/// Session state is an in-memory bearer-token map, which is fine for a single-instance demo.
public class DevAuthService(AppDbContext db) : IDevAuthService
{
    private static readonly ConcurrentDictionary<string, string> PendingTokens = new(); // token -> contact
    private static readonly ConcurrentDictionary<string, Guid> BearerSessions = new(); // bearer -> userId

    public async Task<MagicLinkRequest> StartAsync(string contact, CancellationToken ct = default)
    {
        var token = Guid.NewGuid().ToString("N");
        PendingTokens[token] = contact;
        await Task.CompletedTask;
        return new MagicLinkRequest(token, contact);
    }

    public async Task<(string BearerToken, User User)?> CompleteAsync(string token, CancellationToken ct = default)
    {
        if (!PendingTokens.TryRemove(token, out var contact))
        {
            return null;
        }

        var isEmail = contact.Contains('@');
        var user = await db.Users.FirstOrDefaultAsync(
            u => isEmail ? u.Email == contact : u.Phone == contact, ct);

        if (user is null)
        {
            user = new User
            {
                Email = isEmail ? contact : null,
                Phone = isEmail ? null : contact,
                DisplayName = isEmail ? contact.Split('@')[0] : contact,
                AuthProvider = isEmail ? AuthProvider.MagicLink : AuthProvider.Phone
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }

        var bearer = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        BearerSessions[bearer] = user.Id;
        return (bearer, user);
    }

    public async Task<User?> ResolveBearerTokenAsync(string bearerToken, CancellationToken ct = default)
    {
        if (!BearerSessions.TryGetValue(bearerToken, out var userId))
        {
            return null;
        }
        return await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    }
}
