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

    // Last-used checkout delivery info (see #304) — prefills the checkout form for returning
    // customers. Always written together (CheckoutCommand/CheckoutBatchCommand, on success only),
    // so RecipientName being non-null is the signal the other four fields are populated too.
    public string? LastDeliveryRecipientName { get; set; }
    public string? LastDeliveryPhone { get; set; }
    public string? LastDeliveryCity { get; set; }
    public string? LastDeliveryWarehouseNumber { get; set; }
    public string? LastDeliveryWarehouseAddress { get; set; }

    // Checkout phone verification (#304) — scoped to the user rather than an order so it works
    // uniformly for both single-order and batch checkout. PhoneVerificationPhone is the number the
    // current code was sent to (set immediately on send); PhoneVerifiedPhone is only set once that
    // code is confirmed, and is cleared whenever a new code is sent to a (possibly different)
    // number — CheckoutCommand/CheckoutBatchCommand require it to match the delivery phone exactly.
    public string? PhoneVerificationPhone { get; set; }
    public string? PhoneVerificationCode { get; set; }
    public DateTime? PhoneVerificationCodeExpiresAtUtc { get; set; }
    public int PhoneVerificationAttempts { get; set; }
    public string? PhoneVerifiedPhone { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
