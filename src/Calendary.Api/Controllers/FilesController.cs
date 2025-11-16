using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : BaseUserController
{
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileUploadService fileUploadService,
        IUserService userService,
        ILogger<FilesController> logger) : base(userService)
    {
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single file
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<FileUploadResponse>> UploadFile(IFormFile file)
    {
        try
        {
            var currentUser = await CurrentUser.Value;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            var uploadedFile = await _fileUploadService.UploadAsync(file, currentUser.Id);

            return Ok(new FileUploadResponse
            {
                Id = uploadedFile.Id,
                Url = uploadedFile.Url,
                ThumbnailUrl = uploadedFile.ThumbnailUrl,
                FileName = uploadedFile.FileName,
                FileSize = uploadedFile.FileSize
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    /// <summary>
    /// Upload multiple files
    /// </summary>
    [HttpPost("upload/multiple")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB total
    public async Task<ActionResult<List<FileUploadResponse>>> UploadMultipleFiles(IFormFileCollection files)
    {
        try
        {
            var currentUser = await CurrentUser.Value;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest("No files provided");
            }

            if (files.Count > 10)
            {
                return BadRequest("Maximum 10 files can be uploaded at once");
            }

            var uploadedFiles = await _fileUploadService.UploadMultipleAsync(files, currentUser.Id);

            var responses = uploadedFiles.Select(f => new FileUploadResponse
            {
                Id = f.Id,
                Url = f.Url,
                ThumbnailUrl = f.ThumbnailUrl,
                FileName = f.FileName,
                FileSize = f.FileSize
            }).ToList();

            return Ok(responses);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading files");
            return StatusCode(500, "An error occurred while uploading files");
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFile(int id)
    {
        try
        {
            var currentUser = await CurrentUser.Value;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            await _fileUploadService.DeleteAsync(id, currentUser.Id);

            return NoContent();
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found: {FileId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", id);
            return StatusCode(500, "An error occurred while deleting the file");
        }
    }

    /// <summary>
    /// Get file URL by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<string>> GetFileUrl(int id)
    {
        try
        {
            var url = await _fileUploadService.GetUrlAsync(id);
            return Ok(new { url });
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found: {FileId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file URL {FileId}", id);
            return StatusCode(500, "An error occurred while retrieving the file URL");
        }
    }
}
