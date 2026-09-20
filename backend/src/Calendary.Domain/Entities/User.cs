using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? PasswordHash { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool EmailConfirmed { get; set; }
    public string? EmailConfirmationCode { get; set; }
    public DateTime? EmailConfirmationCodeExpiresAtUtc { get; set; }
    // Failed confirm-email attempts against the current code — reset to 0 whenever a fresh code
    // is issued (register/resend) or confirmation succeeds. At 5, ConfirmEmailCommand invalidates
    // the code outright rather than letting it keep being guessed (see #301).
    public int EmailConfirmationAttempts { get; set; }

    // Hashed like UserSession.TokenHash (SHA-256) rather than stored raw, so a DB read alone can't
    // be used to reset someone's password — see PasswordAuthService.GeneratePasswordResetTokenAsync.
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAtUtc { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
