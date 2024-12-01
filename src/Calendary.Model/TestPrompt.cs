namespace Calendary.Model;

public class TestPrompt
{
    public int Id { get; set; }
    public int PromptId { get; set; }
    public int TrainingId { get; set; }
    public string Text { get; set; } = string.Empty;

    public int? Seed { get; set; } // Seed для генерації
    public int? OutputSeed { get; set; } // Seed, який використаний у результаті

    public string? ReplicateId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ProcessedImageUrl { get; set; }
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, ready, failed


    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Prompt Prompt { get; set; } = null!;
    public Training Training { get; set; } = null!;

}
