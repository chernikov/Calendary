namespace Calendary.Api.Dtos;

public class JobDto
{
    public int Id { get; set; }
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, ready, failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
