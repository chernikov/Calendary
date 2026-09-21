using Calendary.Domain.Enums;

namespace Calendary.Domain.Abstractions;

/// <param name="SceneText">Free-typed English scene descriptor — not read from the Prompt library.</param>
/// <param name="StyleText">Free-typed English visual-style descriptor — not read from the ImageStyle library.</param>
/// <param name="Kind">Cover uses BuildCoverPrompt; Month uses BuildMonthPrompt (needs Month).</param>
/// <param name="Month">Required (1–12) when Kind is Month; ignored for Cover.</param>
/// <param name="ReferencePhotoContent">The admin-uploaded reference photo, full-size — downscaled internally the same way order generation does.</param>
/// <param name="Provider">OpenAI or Gemini — Mock is rejected, there is nothing real to test.</param>
public record ExperimentalGenerationRequest(
    string SceneText,
    string StyleText,
    SheetKind Kind,
    int? Month,
    byte[] ReferencePhotoContent,
    string ReferencePhotoContentType,
    ImageGenerationProvider Provider);

public record ExperimentalGenerationResult(bool Success, string? ImageDataUrl, string? Error, decimal? EstimatedCostUsd);

/// Admin-panel "experimental generation" tool (#440) — runs a single real generation call through
/// the exact same prompt-building and AI-client code path production orders use, for diagnosing
/// prompt/style quality issues, but as a pure throwaway: it never touches Prompt/ImageStyle/Order/
/// Sheet rows, and the result is returned inline as a data: URL rather than saved to IFileStorage.
public interface IExperimentalGenerationService
{
    Task<ExperimentalGenerationResult> GenerateAsync(ExperimentalGenerationRequest request, CancellationToken ct = default);
}
