namespace Calendary.Domain.Entities;

/// A visual style (фотореалізм, графіка, ч/б, мультяшний…) overlaid on the chosen prompt's
/// scene. Text is the English style descriptor appended to the generation prompt.
public class ImageStyle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Text { get; set; } = default!;
    public int SortOrder { get; set; }
}
