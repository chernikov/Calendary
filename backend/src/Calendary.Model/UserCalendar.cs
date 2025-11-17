using System;

namespace Calendary.Model;

public class UserCalendar
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? TemplateId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string DesignData { get; set; } = "{}"; // JSON with canvas state

    public string? PreviewImageUrl { get; set; }

    public CalendarStatus Status { get; set; } = CalendarStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public User User { get; set; } = null!;
    public Template? Template { get; set; }
}
