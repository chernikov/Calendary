namespace Calendary.Model.Messages;

public class JobTaskMessage
{
    public int Id { get; set; }

    public int PromptId { get; set; }
    public int FluxModelId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ProcessedImageUrl { get; set; }
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, ready, failed
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
