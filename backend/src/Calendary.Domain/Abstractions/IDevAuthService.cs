using Calendary.Domain.Entities;

namespace Calendary.Domain.Abstractions;

public record MagicLinkRequest(string Token, string Contact);

public interface IDevAuthService
{
    /// Creates (or finds) a user for the given contact and mints a one-time login token.
    /// In production this token would be emailed/texted; here it is simply returned to the
    /// caller so the frontend can simulate "opening the link".
    Task<MagicLinkRequest> StartAsync(string contact, CancellationToken ct = default);

    /// Exchanges a one-time token for a bearer session token + the resolved user.
    Task<(string BearerToken, User User)?> CompleteAsync(string token, CancellationToken ct = default);

    Task<User?> ResolveBearerTokenAsync(string bearerToken, CancellationToken ct = default);
}
