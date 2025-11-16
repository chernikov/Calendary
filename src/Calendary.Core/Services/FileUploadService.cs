using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Calendary.Core.Services;

public interface IFileUploadService
{
    Task<UploadedFile> UploadAsync(IFormFile file, int userId, CancellationToken ct = default);
    Task<List<UploadedFile>> UploadMultipleAsync(IFormFileCollection files, int userId, CancellationToken ct = default);
    Task DeleteAsync(int fileId, int userId);
    Task<string> GetUrlAsync(int fileId);
}

public class FileUploadService : IFileUploadService
{
    private readonly IUploadedFileRepository _fileRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<FileUploadService> _logger;
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

    public FileUploadService(
        IUploadedFileRepository fileRepository,
        IConfiguration config,
        ILogger<FileUploadService> logger,
        IWebHostEnvironment environment)
    {
        _fileRepository = fileRepository;
        _config = config;
        _logger = logger;
        _environment = environment;
    }

    public async Task<UploadedFile> UploadAsync(IFormFile file, int userId, CancellationToken ct = default)
    {
        ValidateFile(file);

        // Generate unique file name
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        // Ensure uploads directory exists
        var uploadsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        // Save file to disk
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        // TODO: In production, implement image processing with ImageSharp:
        // - Resize to max dimensions (4000x4000)
        // - Optimize quality
        // - Generate thumbnail (300x300)
        // - Convert to WebP for web optimization
        // For now, we'll save the original file

        var uploadedFile = new UploadedFile
        {
            UserId = userId,
            FileName = file.FileName,
            FileSize = file.Length,
            MimeType = file.ContentType,
            Url = $"/uploads/{uniqueFileName}",
            ThumbnailUrl = $"/uploads/{uniqueFileName}", // Same as original for now
            CreatedAt = DateTime.UtcNow
        };

        await _fileRepository.AddAsync(uploadedFile);

        _logger.LogInformation("File uploaded: {FileName} by user {UserId}", file.FileName, userId);

        return uploadedFile;
    }

    public async Task<List<UploadedFile>> UploadMultipleAsync(
        IFormFileCollection files,
        int userId,
        CancellationToken ct = default)
    {
        var uploadedFiles = new List<UploadedFile>();

        foreach (var file in files)
        {
            try
            {
                var uploadedFile = await UploadAsync(file, userId, ct);
                uploadedFiles.Add(uploadedFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                // Continue with other files
            }
        }

        return uploadedFiles;
    }

    public async Task DeleteAsync(int fileId, int userId)
    {
        var file = await _fileRepository.GetByIdAndUserIdAsync(fileId, userId);

        if (file == null)
        {
            throw new FileNotFoundException($"File with ID {fileId} not found");
        }

        // Soft delete in database
        await _fileRepository.SoftDeleteAsync(fileId, userId);

        // TODO: Optionally delete physical file from disk
        // For now, we keep files on disk for recovery purposes

        _logger.LogInformation("File deleted: {FileId} by user {UserId}", fileId, userId);
    }

    public async Task<string> GetUrlAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);

        if (file == null)
        {
            throw new FileNotFoundException($"File with ID {fileId} not found");
        }

        return file.Url;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
        }

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            throw new ArgumentException($"MIME type {file.ContentType} is not allowed");
        }
    }
}

// Note: IWebHostEnvironment needs to be available
// Add this using directive at the top: using Microsoft.AspNetCore.Hosting;
namespace Microsoft.AspNetCore.Hosting
{
    // Placeholder interface if not available
    public interface IWebHostEnvironment
    {
        string WebRootPath { get; }
        string EnvironmentName { get; }
    }
}
