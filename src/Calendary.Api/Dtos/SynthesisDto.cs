namespace Calendary.Api.Dtos;

public class SynthesisDto 
{
    public int Id { get; set; }
    public int PromptId { get; set; }

    public int TrainingId { get; set; }
    public string Text { get; set; } = string.Empty;

    public int? Seed { get; set; }

    public int? OutputSeed { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public string? ProcessedImageUrl { get; set; }
    public string Status { get; set; } = string.Empty; 
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public PromptDto Prompt { get; set; } = null!;
}
