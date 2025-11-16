using System;
using System.Collections.Generic;

namespace Calendary.Model;

public class Template
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string PreviewImageUrl { get; set; } = string.Empty;

    public string TemplateData { get; set; } = "{}"; // JSON with layout data

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<UserCalendar> UserCalendars { get; set; } = new List<UserCalendar>();
}
