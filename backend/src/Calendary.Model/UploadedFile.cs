using System;

namespace Calendary.Model;

public class UploadedFile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string MimeType { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    // Navigation property
    public User User { get; set; } = null!;
}
