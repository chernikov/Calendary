using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Job
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ThemeId { get; set; }
    public int FluxModelId { get; set; }
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, ready, failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Навігаційні властивості
    public User User { get; set; } = null!;
    public PromptTheme Theme { get; set; } = null!;
    public FluxModel FluxModel { get; set; } = null!;
    public ICollection<JobTask> Tasks { get; set; } = new List<JobTask>();
}

