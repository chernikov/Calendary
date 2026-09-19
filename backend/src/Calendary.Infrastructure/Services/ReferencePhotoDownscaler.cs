using Calendary.Domain.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Calendary.Infrastructure.Services;

/// Both providers take the reference photo inline (multipart for OpenAI, base64 JSON for Gemini)
/// and it is re-sent once per sheet, so a 10 MB original would be uploaded 13 times per order.
/// Shrinking it into a ~200–500 KB band keeps request latency and cost down without hurting
/// likeness — only the generated sheets need to come back at full resolution.
public static class ReferencePhotoDownscaler
{
    public const int TargetMaxBytes = 500 * 1024;

    private static readonly int[] MaxDimensions = [1536, 1280, 1024, 768];
    private static readonly int[] Qualities = [85, 75, 65];

    public static StoredFile Downscale(StoredFile original)
    {
        if (original.Content.Length <= TargetMaxBytes)
        {
            return original;
        }

        using var image = Image.Load(original.Content);

        // Phone photos carry their rotation in EXIF, which is dropped on re-encode below.
        image.Mutate(c => c.AutoOrient());
        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;

        StoredFile? smallest = null;
        foreach (var maxDimension in MaxDimensions)
        {
            using var resized = image.Width <= maxDimension && image.Height <= maxDimension
                ? image.Clone(_ => { })
                : image.Clone(c => c.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxDimension, maxDimension),
                }));

            foreach (var quality in Qualities)
            {
                using var buffer = new MemoryStream();
                resized.Save(buffer, new JpegEncoder { Quality = quality });
                smallest = new StoredFile(buffer.ToArray(), "image/jpeg");

                if (smallest.Content.Length <= TargetMaxBytes)
                {
                    return smallest;
                }
            }
        }

        return smallest!;
    }
}
