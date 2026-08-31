namespace Calendary.Api.Photos;

public readonly record struct PhotoIntakeResult(byte[] Bytes, string ContentType, string? Error)
{
    public bool Ok => Error is null;

    public static PhotoIntakeResult Fail(string error) => new([], string.Empty, error);
}

/// Codes rather than prose: all user-facing wording is Ukrainian and lives in the frontend.
public static class PhotoIntakeError
{
    public const string Required = "photo_required";
    public const string TooLarge = "photo_too_large";
    public const string UnsupportedFormat = "photo_unsupported_format";
}

/// Shared validation for the two endpoints that accept a customer photo as a multipart file
/// upload (OrdersController.UploadPhoto and AdminController.ReplacePhoto).
public static class PhotoIntake
{
    public const int MaxBytes = 20 * 1024 * 1024;

    public static async Task<PhotoIntakeResult> ReadAsync(IFormFile? photo, CancellationToken ct = default)
    {
        if (photo is null || photo.Length == 0)
        {
            return PhotoIntakeResult.Fail(PhotoIntakeError.Required);
        }

        if (photo.Length > MaxBytes)
        {
            return PhotoIntakeResult.Fail(PhotoIntakeError.TooLarge);
        }

        using var buffer = new MemoryStream();
        await photo.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();

        // The declared Content-Type is client-supplied, so the format is taken from the actual
        // bytes instead — this is what decides the extension the file is stored and served under.
        var contentType = DetectContentType(bytes);
        if (contentType is null)
        {
            return PhotoIntakeResult.Fail(PhotoIntakeError.UnsupportedFormat);
        }

        return new PhotoIntakeResult(bytes, contentType, null);
    }

    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static string? DetectContentType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(PngSignature))
        {
            return "image/png";
        }

        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }
}
