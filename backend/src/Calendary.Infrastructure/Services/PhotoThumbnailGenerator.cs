using Calendary.Domain.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Calendary.Infrastructure.Services;

/// Small preview for the upload-step photo grid and admin thumbnails — decoupled from
/// ReferencePhotoDownscaler's ~500KB "generation reference" target, which is still far too large
/// to ship many-at-once to the browser as a UI thumbnail.
public static class PhotoThumbnailGenerator
{
    private const int MaxDimension = 320;
    private const int Quality = 75;

    public static StoredFile Generate(StoredFile original)
    {
        using var image = Image.Load(original.Content);

        // Phone photos carry their rotation in EXIF, which is dropped on re-encode below.
        image.Mutate(c => c.AutoOrient());
        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;

        using var resized = image.Width <= MaxDimension && image.Height <= MaxDimension
            ? image.Clone(_ => { })
            : image.Clone(c => c.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxDimension, MaxDimension),
            }));

        using var buffer = new MemoryStream();
        resized.Save(buffer, new JpegEncoder { Quality = Quality });
        return new StoredFile(buffer.ToArray(), "image/jpeg");
    }
}
