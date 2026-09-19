namespace Calendary.Domain.Entities;

/// One generated image for a Sheet. Every successful generation adds a row here rather than
/// overwriting Sheet.ImageUrl, so a customer can browse back through past results and restore one
/// as active without re-generating.
public class SheetVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SheetId { get; set; }
    public Sheet Sheet { get; set; } = default!;

    public string ImageUrl { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
