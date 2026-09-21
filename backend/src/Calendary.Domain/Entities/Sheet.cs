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

    // Manual reference-photo override (see #351) — null means "use the first uploaded photo."
    public Guid? PinnedPhotoId { get; set; }
    public OrderPhoto? PinnedPhoto { get; set; }

    public SheetStatus Status { get; set; } = SheetStatus.Pending;
    public bool IsSelected { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? GeneratingStartedAtUtc { get; set; }
    public DateTime? ReadyAtUtc { get; set; }

    /// User-facing explanation for the most recent Failed status (see #401) — e.g. distinguishing
    /// an AI provider content-safety rejection (needs a different photo/prompt) from a generic
    /// transient failure (just retry). Null once the sheet is Ready or hasn't failed yet.
    public string? FailureReason { get; set; }

    // ImageUrl/Status/ReadyAtUtc above always mirror ActiveVariant — kept denormalized so every
    // existing reader (PDF service, DtoMapping, admin panel) needs no changes. Variants is the
    // full generation history; ActiveVariantId is which one is "the" current result.
    public Guid? ActiveVariantId { get; set; }
    public SheetVariant? ActiveVariant { get; set; }
    public ICollection<SheetVariant> Variants { get; set; } = new List<SheetVariant>();
}
