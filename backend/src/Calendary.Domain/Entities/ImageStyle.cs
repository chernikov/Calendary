namespace Calendary.Domain.Entities;

/// A visual style (фотореалізм, графіка, ч/б, мультяшний…) overlaid on the chosen prompt's
/// scene. Text is the English style descriptor appended to the generation prompt.
public class ImageStyle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Text { get; set; } = default!;
    /// Short Ukrainian teaser (5–10 words) shown in the picker; not the generation prompt.
    public string Description { get; set; } = "";
    public string? PreviewImageUrl { get; set; }
    public int SortOrder { get; set; }
}
