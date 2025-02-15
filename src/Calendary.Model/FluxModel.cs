﻿namespace Calendary.Model;

public class FluxModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReplicateId { get; set; } = string.Empty;
    public string Status { get; set; } = "prepared"; 

    public int? CategoryId { get; set; } 
    public string? ArchiveUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public bool IsPaid { get; set; } // Поле оплати

    public bool IsArchive { get; set; }

    public bool IsDeleted { get; set; }

    // Навігаційні властивості

    public WebHook? WebHook { get; set; }
    
    public User User { get; set; } = null!;

    public Category? Category { get; set; }
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    public ICollection<Training> Trainings { get; set; } = new List<Training>();
    public ICollection<JobTask> Tasks { get; set; } = new List<JobTask>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
