using Calendary.Core.Providers;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace Calendary.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/user-upload-photo")]
public class UserUploadPhotoController : BaseAdminController
{
    private readonly string _uploadDirectory = "uploads";
    private readonly IPathProvider _pathProvider;
    private readonly IFluxModelService _fluxModelService;
    private readonly IPhotoService _photoService;

    public UserUploadPhotoController(
        IPathProvider pathProvider,
        IUserService userService,
        IFluxModelService fluxModelService,
        IPhotoService photoService) : base(userService)
    {
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }

        _pathProvider = pathProvider;
        _fluxModelService = fluxModelService;
        _photoService = photoService;
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto([FromForm] int fluxModelId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Перевірка FluxModel
        var fluxModel = await _fluxModelService.GetByIdAsync(fluxModelId);
        if (fluxModel == null)
        {
            return NotFound($"FluxModel with ID {fluxModelId} not found.");
        }

        var uploadDirectory = Path.Combine(_uploadDirectory, fluxModelId.ToString());
        var realUploadDirectory = _pathProvider.MapPath(uploadDirectory);
        if (!Directory.Exists(realUploadDirectory))
        {
            Directory.CreateDirectory(realUploadDirectory);
        }

        var filePath = await SaveFileAsync(file, uploadDirectory);

        // Додати запис у базу даних
        var photo = new Photo
        {
            FluxModelId = fluxModelId,
            ImageUrl = filePath
        };

        await _photoService.SaveAsync(photo);

        return Ok(new { Message = "File uploaded successfully.", FilePath = filePath });
    }


    [Authorize(Roles ="Admin")]
    [HttpGet("complete/{fluxModelId:int}")]
    public async Task<IActionResult> Complete(int fluxModelId)
    {
        // Отримуємо модель з бази даних
        var model = await _fluxModelService.GetByIdAsync(fluxModelId);
        if (model == null)
        {
            return NotFound($"Model with ID {fluxModelId} not found.");
        }

        var photos = await _photoService.GetByFluxModelIdAsync(model.Id);
        // Отримуємо файли, пов’язані з моделлю
        var files = photos.Select(p => p.ImageUrl); // Це список шляхів до файлів
        if (files == null || !files.Any())
        {
            return BadRequest("No files are associated with this model.");
        }

        // Створюємо ZIP-архів
        var zipFileName = $"model_{fluxModelId}_archive.zip";
        var zipFilePath = Path.Combine(_uploadDirectory, zipFileName);
        var realZipFilePath = _pathProvider.MapPath(zipFilePath); // Змінити на реальний шлях


        var index = 1;
        using (var zipStream = new FileStream(realZipFilePath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false))
        {
            foreach (var filePath in files)
            {
                var realFilePath = _pathProvider.MapPath(filePath);
                if (System.IO.File.Exists(realFilePath))
                {
                    var fileName = Path.GetFileName(realFilePath);
                    var extension = Path.GetExtension(realFilePath);
                    archive.CreateEntryFromFile(realFilePath, $"TOK({index}){extension}");
                }
                index++;
            }
        }

        // Оновлюємо модель: зберігаємо шлях до архіву і змінюємо статус
        zipFilePath = zipFilePath.Replace("\\", "/");
        model.ArchiveUrl = zipFilePath;
        await _fluxModelService.UpdateArchiveUrlAsync(model);
        model.Status = "pay_model";
        await _fluxModelService.UpdateStatusAsync(model);

        return Ok(new { Message = "Model updated and archived successfully.", ZipFile = zipFilePath });
    }
    private async Task<string> SaveFileAsync(IFormFile file, string realUploadDir)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(realUploadDir, fileName);
        var realFilePath = _pathProvider.MapPath(filePath);
        using (var stream = new FileStream(realFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;
    }
}