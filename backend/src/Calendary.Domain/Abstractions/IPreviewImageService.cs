namespace Calendary.Domain.Abstractions;

/// Admin-panel "generate example" for a Prompt/ImageStyle (#314) — generates a standalone image
/// (no customer reference photo) via whichever real AI provider is currently selected, so admins
/// can preview a scene/style combination before any customer orders it.
public interface IPreviewImageService
{
    /// Returns the saved image's URL. Throws AppOperationException if the Mock provider is active
    /// (nothing real to generate) or the provider call fails.
    Task<string> GeneratePreviewAsync(string sceneText, string styleText, CancellationToken ct = default);
}
