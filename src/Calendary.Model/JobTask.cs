using System.Text.Json.Serialization;

namespace Calendary.Model;

public class JobTask
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int FluxModelId { get; set; }
    public int PromptId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ProcessedImageUrl { get; set; }
    public int? Seed { get; set; } // Seed для генерації
    public int? OutputSeed { get; set; } // Seed, який використаний у результаті

    public string? ReplicateId { get; set; }
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, ready, failed
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Навігаційні властивості

    [JsonIgnore]
    public Job Job { get; set; } = null!;

    [JsonIgnore]
    public FluxModel FluxModel { get; set; } = null!;
    public Prompt Prompt { get; set; } = null!;
}
