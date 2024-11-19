namespace Calendary.Model;

public class Photo
{
    public int Id { get; set; }
    public int FluxModelId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string? ProcessedImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навігаційні властивості
    public FluxModel FluxModel { get; set; } = null!;
}