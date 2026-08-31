namespace Calendary.Domain.Abstractions;

public record StoredFile(byte[] Content, string ContentType);

/// Binary storage for uploaded photos and generated sheets. Everything that used to be inlined
/// into the database as a base64 data: URL lives here instead; entities keep only the returned
/// public URL.
public interface IFileStorage
{
    /// <param name="category">Bucket name, e.g. "photos" or "sheets". Must be a simple slug.</param>
    /// <returns>An app-relative public URL (e.g. "/api/media/photos/ab12….png").</returns>
    Task<string> SaveAsync(byte[] content, string contentType, string category, CancellationToken ct = default);

    /// Reads back a file previously written by <see cref="SaveAsync"/>.
    Task<StoredFile> ReadAsync(string url, CancellationToken ct = default);

    /// True when the URL was produced by this storage, as opposed to a data: URL or an external
    /// http(s) one (the mock generator still emits those).
    bool IsStoredUrl(string? url);
}
