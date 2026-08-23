using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? DisplayName { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
