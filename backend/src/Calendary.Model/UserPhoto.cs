using System.Text.Json.Serialization;

namespace Calendary.Model;

public class UserPhoto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public string? OriginalFileName { get; set; }

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [JsonIgnore]
    public User User { get; set; } = null!;
}
