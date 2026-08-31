using Calendary.Domain;

namespace Calendary.Api.Photos;

/// Shared validation for the two endpoints that accept a customer photo as a data: URL
/// (OrdersController.UploadPhoto and AdminController.ReplacePhoto).
public static class PhotoIntake
{
    public const int MaxBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    public static bool TryDecode(string? photoDataUrl, out byte[] bytes, out string contentType, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(photoDataUrl))
        {
            bytes = [];
            contentType = string.Empty;
            error = "photoDataUrl is required.";
            return false;
        }

        if (!DataUrl.TryParse(photoDataUrl, out contentType, out bytes))
        {
            error = "photoDataUrl must be a base64 data URL.";
            return false;
        }

        if (!AllowedContentTypes.Contains(contentType))
        {
            error = "Only JPEG, PNG and WebP images are supported.";
            return false;
        }

        if (bytes.Length > MaxBytes)
        {
            error = $"Image must be at most {MaxBytes / (1024 * 1024)} MB.";
            return false;
        }

        return true;
    }
}
