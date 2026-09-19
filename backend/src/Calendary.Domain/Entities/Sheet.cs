using Calendary.Domain.Enums;

namespace Calendary.Domain.Entities;

public class Sheet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public SheetKind Kind { get; set; }
    public int Index { get; set; } // 0 = cover, 1..12 = month number

    // The user's per-sheet picks: scene (prompt) + visual style, chosen before generation.
    public Guid? PromptId { get; set; }
    public Prompt? Prompt { get; set; }
    public Guid? ImageStyleId { get; set; }
    public ImageStyle? ImageStyle { get; set; }

    public SheetStatus Status { get; set; } = SheetStatus.Pending;
    public bool IsSelected { get; set; }
    public string? ImageUrl { get; set; }
    public int VariantCount { get; set; }
    public DateTime? GeneratingStartedAtUtc { get; set; }
    public DateTime? ReadyAtUtc { get; set; }
}
