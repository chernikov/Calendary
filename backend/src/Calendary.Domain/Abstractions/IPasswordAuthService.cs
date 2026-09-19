using Calendary.Domain.Entities;

namespace Calendary.Domain.Abstractions;

public interface IPasswordAuthService
{
    /// Returns null if the email is already registered.
    Task<User?> RegisterAsync(string email, string password, string? displayName, CancellationToken ct = default);

    /// Returns null for either "no such user" or "wrong password" — deliberately the same
    /// signal for both, so callers can't use this to enumerate registered emails.
    Task<User?> LoginAsync(string email, string password, CancellationToken ct = default);
}
