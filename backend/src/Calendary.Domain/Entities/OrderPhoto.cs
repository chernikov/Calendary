namespace Calendary.Domain.Entities;

public class OrderPhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    // Full/downscaled-for-reference image, used as the AI generation input.
    public string Url { get; set; } = default!;
    // Small preview for the upload-step grid and admin thumbnails.
    public string ThumbUrl { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
