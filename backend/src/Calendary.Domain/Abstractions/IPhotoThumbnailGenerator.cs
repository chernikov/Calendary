namespace Calendary.Domain.Abstractions;

/// Lets Application-layer command handlers generate an upload-step preview thumbnail without
/// depending on Infrastructure's concrete SixLabors.ImageSharp-based implementation directly.
public interface IPhotoThumbnailGenerator
{
    StoredFile Generate(StoredFile original);
}
