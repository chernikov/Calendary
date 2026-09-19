using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

/// Stores files on the local filesystem under FileStorageOptions.RootPath, served back by the
/// static-file middleware mounted at FileStorageOptions.PublicBasePath (see Program.cs).
public class LocalFileStorage : IFileStorage
{
    private static readonly Dictionary<string, string> ExtensionByContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };

    private readonly FileStorageOptions _options;
    private readonly string _rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        _rootPath = Path.GetFullPath(_options.ResolveRootPath(environment.ContentRootPath));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(byte[] content, string contentType, string category, CancellationToken ct = default)
    {
        if (!ExtensionByContentType.TryGetValue(contentType, out var extension))
        {
            throw new NotSupportedException($"Unsupported content type '{contentType}'.");
        }
        if (!IsSafeCategory(category))
        {
            throw new ArgumentException("Category must be a lowercase slug.", nameof(category));
        }

        var directory = Path.Combine(_rootPath, category);
        Directory.CreateDirectory(directory);

        // Filenames are always server-generated: nothing from the request reaches the path.
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(directory, fileName), content, ct);

        return $"{_options.PublicBasePath}/{category}/{fileName}";
    }

    public async Task<StoredFile> ReadAsync(string url, CancellationToken ct = default)
    {
        var path = ResolvePhysicalPath(url);
        var content = await File.ReadAllBytesAsync(path, ct);
        var contentType = ExtensionByContentType
            .FirstOrDefault(pair => pair.Value.Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase))
            .Key ?? "application/octet-stream";

        return new StoredFile(content, contentType);
    }

    public bool IsStoredUrl(string? url) =>
        url is not null && url.StartsWith(_options.PublicBasePath + "/", StringComparison.Ordinal);

    private string ResolvePhysicalPath(string url)
    {
        if (!IsStoredUrl(url))
        {
            throw new ArgumentException($"'{url}' is not a stored file URL.", nameof(url));
        }

        var relative = url[(_options.PublicBasePath.Length + 1)..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relative));

        // Defence in depth against traversal even though URLs come from our own database.
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException($"'{url}' resolves outside the storage root.");
        }

        return fullPath;
    }

    private static bool IsSafeCategory(string category) =>
        category.Length > 0 && category.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '-');
}
