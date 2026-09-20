namespace Calendary.Domain.Abstractions;

public interface ISmsService
{
    /// Sends a phone-verification code SMS and returns the code that was actually sent, so the
    /// caller can persist it for later comparison — mirrors
    /// IPasswordAuthService.GeneratePasswordResetTokenAsync's "returns what it generated" shape.
    /// The implementation itself decides fixed vs. real-random (see SmsClubService) based on
    /// whether a real provider is configured, so callers never need to know that (#304).
    Task<string> SendVerificationCodeAsync(string phone, CancellationToken ct = default);
}
