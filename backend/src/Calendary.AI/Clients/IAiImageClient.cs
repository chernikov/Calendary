namespace Calendary.AI.Clients;

/// <param name="Prompt">The full text prompt (see Prompts/CalendarPrompts.cs).</param>
/// <param name="ReferencePhotoDataUrl">
/// The customer's uploaded photo as a data: URL (downscaled by ReferencePhotoDownscaler before
/// it gets here), used for image-to-image generation so the result keeps their likeness. Both
/// providers support this.
/// </param>
public record AiImageRequest(string Prompt, string? ReferencePhotoDataUrl);

/// <param name="ImageDataUrl">The generated image, base64-encoded as a data: URL, on success.</param>
/// <param name="EstimatedCostUsd">
/// A static per-provider/model/quality estimate (see AiOptions), not parsed from the provider's
/// response — neither OpenAI's nor Gemini's image APIs return per-call billing data. Recorded on
/// the resulting SheetVariant purely for cost monitoring (see #359); update the config value when
/// the configured model/quality/pricing changes.
/// </param>
public record AiImageResult(bool Success, string? ImageDataUrl, string? Error, decimal? EstimatedCostUsd = null);

/// One implementation per provider (OpenAI, Gemini) — the active one is chosen by
/// AiOptions.Provider and registered as the sole IAiImageClient in DI (see
/// ServiceCollectionExtensions.AddCalendaryAi).
public interface IAiImageClient
{
    Task<AiImageResult> GenerateImageAsync(AiImageRequest request, CancellationToken ct = default);
}
