using Calendary.Domain.Entities;

namespace Calendary.Domain.Abstractions;

public interface ISessionTokenService
{
    /// Mints a new opaque bearer token for the user and persists it (hashed) as a UserSession.
    Task<string> IssueTokenAsync(User user, CancellationToken ct = default);

    /// Resolves a bearer token back to its user, or null if it's invalid/expired.
    Task<User?> ResolveAsync(string bearerToken, CancellationToken ct = default);

    /// Revokes every session for a user — called after a password reset so a stolen bearer token
    /// stops working the moment the legitimate owner regains control of the account (#301).
    Task InvalidateAllSessionsAsync(Guid userId, CancellationToken ct = default);
}
