using Calendary.Core.Providers;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/user-photo")]
public class UserPhotoController : BaseUserController
{
    private readonly string _uploadDirectory = "uploads/user-photos";
    private readonly IPathProvider _pathProvider;
    private readonly IUserPhotoService _userPhotoService;

    public UserPhotoController(
        IPathProvider pathProvider,
        IUserService userService,
        IUserPhotoService userPhotoService) : base(userService)
    {
        _pathProvider = pathProvider;
        _userPhotoService = userPhotoService;

        var realUploadDirectory = _pathProvider.MapPath(_uploadDirectory);
        if (!Directory.Exists(realUploadDirectory))
        {
            Directory.CreateDirectory(realUploadDirectory);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPhotos()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var photos = await _userPhotoService.GetByUserIdAsync(user.Id);
        return Ok(photos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserPhoto(int id)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var photo = await _userPhotoService.GetByIdAndUserIdAsync(id, user.Id);
        if (photo is null)
        {
            return NotFound($"Photo with ID {id} not found.");
        }

        return Ok(photo);
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file, [FromForm] string? caption)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest("Invalid file type. Only image files are allowed.");
        }

        // Validate file size (max 10MB)
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxFileSize)
        {
            return BadRequest("File size exceeds the maximum allowed size of 10MB.");
        }

        var uploadDirectory = Path.Combine(_uploadDirectory, user.Id.ToString());
        var filePath = await SaveFileAsync(file, uploadDirectory);

        var photo = new UserPhoto
        {
            UserId = user.Id,
            ImageUrl = filePath,
            Caption = caption,
            OriginalFileName = file.FileName,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        await _userPhotoService.CreateAsync(photo);

        return Ok(new { Message = "File uploaded successfully.", Photo = photo });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePhoto(int id, [FromBody] UpdatePhotoRequest request)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var photo = await _userPhotoService.GetByIdAndUserIdAsync(id, user.Id);
        if (photo is null)
        {
            return NotFound($"Photo with ID {id} not found.");
        }

        photo.Caption = request.Caption;
        await _userPhotoService.UpdateAsync(photo);

        return Ok(new { Message = "Photo updated successfully.", Photo = photo });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var photo = await _userPhotoService.GetByIdAndUserIdAsync(id, user.Id);
        if (photo is null)
        {
            return NotFound($"Photo with ID {id} not found.");
        }

        // Soft delete
        await _userPhotoService.SoftDeleteAsync(id, user.Id);

        // Optionally delete the physical file
        try
        {
            var realFilePath = _pathProvider.MapPath(photo.ImageUrl);
            if (System.IO.File.Exists(realFilePath))
            {
                System.IO.File.Delete(realFilePath);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }

        return Ok(new { Message = "Photo deleted successfully." });
    }

    private async Task<string> SaveFileAsync(IFormFile file, string uploadDirectory)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadDirectory, fileName);
        var realFilePath = _pathProvider.MapPath(filePath);

        var directory = Path.GetDirectoryName(realFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var stream = new FileStream(realFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath.Replace("\\", "/");
    }
}

public class UpdatePhotoRequest
{
    public string? Caption { get; set; }
}
