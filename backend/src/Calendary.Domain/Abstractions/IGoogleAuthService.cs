using Calendary.Domain.Entities;

namespace Calendary.Domain.Abstractions;

public class InvalidGoogleTokenException(string message) : Exception(message);

public interface IGoogleAuthService
{
    /// Verifies the Google Identity Services ID token and finds-or-creates the matching User.
    /// Throws InvalidGoogleTokenException if the token doesn't verify.
    Task<User> AuthenticateAsync(string idToken, CancellationToken ct = default);
}
