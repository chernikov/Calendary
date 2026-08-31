using Calendary.Domain.Entities;

namespace Calendary.Domain.Abstractions;

public interface IImageGenerationService
{
    /// Starts generation for the order's 13 pre-planned sheets (cover + 12 months, each carrying
    /// its own Prompt/ImageStyle picks), progressing them from Pending -> Generating -> Ready.
    Task<IReadOnlyList<Sheet>> StartOrderGenerationAsync(Guid orderId, CancellationToken ct = default);

    /// Resets a single sheet back to Pending so the background generator produces a new variant.
    /// Returns false if the order has no regenerations left.
    Task<bool> RegenerateSheetAsync(Guid orderId, Guid sheetId, CancellationToken ct = default);

    /// Generates (or re-generates) one sheet while the order is still in the planning step,
    /// before full-order generation. Does not consume the regeneration budget and does not
    /// change the order status.
    Task GenerateSheetPreviewAsync(Guid orderId, Guid sheetId, CancellationToken ct = default);
}
