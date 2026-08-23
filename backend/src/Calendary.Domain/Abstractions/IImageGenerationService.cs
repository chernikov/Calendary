using Calendary.Domain.Entities;

namespace Calendary.Domain.Abstractions;

public interface IImageGenerationService
{
    /// Creates the 13 sheets (cover + 12 months) for an order and hands them to the
    /// background generator, which progresses them from Pending -> Generating -> Ready over time.
    Task<IReadOnlyList<Sheet>> StartOrderGenerationAsync(Guid orderId, CancellationToken ct = default);

    /// Resets a single sheet back to Pending so the background generator produces a new variant.
    /// Returns false if the order has no regenerations left.
    Task<bool> RegenerateSheetAsync(Guid orderId, Guid sheetId, CancellationToken ct = default);
}
