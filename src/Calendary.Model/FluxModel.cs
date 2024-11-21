using Calendary.Model.Enums;

namespace Calendary.Model;

public class FluxModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReplicateId { get; set; } = string.Empty;
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, failed, ready
    public GenderEnum Gender { get; set; } = GenderEnum.Male; 
    public string ArchiveUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Навігаційні властивості

    public WebHook? WebHook { get; set; }
    
    public User User { get; set; } = null!;

    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    public ICollection<Training> Trainings { get; set; } = new List<Training>();
    public ICollection<JobTask> Tasks { get; set; } = new List<JobTask>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
