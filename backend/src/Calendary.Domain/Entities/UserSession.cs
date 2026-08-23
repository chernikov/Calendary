namespace Calendary.Domain.Entities;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    /// SHA-256 hash of the bearer token — the raw token is never persisted, so a DB
    /// backup/leak can't be used to impersonate an active session.
    public string TokenHash { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddDays(60);
}
