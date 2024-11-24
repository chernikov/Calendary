namespace Calendary.Api.Dtos;

public class TrainingDto
{
    public int Id { get; set; }
    public string ReplicateId { get; set; } = string.Empty;
    public string Status { get; set; } = "starting"; // ENUM: starting, inprogress, completed, failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
