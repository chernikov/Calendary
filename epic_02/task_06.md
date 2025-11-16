# Task 06: File Upload Service для зображень

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Claude
**Паралельно з**: Task 01, 02, 03, 04, 05

## Опис задачі

Створити сервіс для завантаження, обробки та зберігання зображень користувачів (фото для календарів, preview images).

## Проблема

Користувачі повинні мати можливість завантажувати свої фото для додавання в календарі. Потрібна обробка, оптимізація та безпечне зберігання.

## Що треба зробити

1. **Створити FileUploadService**
   - Прийом файлів через API
   - Валідація типу, розміру
   - Генерація унікальних імен
   - Збереження в blob storage

2. **Додати обробку зображень**
   - Використовувати ImageSharp для .NET
   - Resize до максимальних розмірів (4000x4000px)
   - Оптимізація якості (compression)
   - Генерація thumbnails (300x300px)
   - Конвертація в WEBP для web

3. **Створити API endpoints**
   - `POST /api/files/upload` - завантажити файл
   - `POST /api/files/upload/multiple` - множинне завантаження
   - `DELETE /api/files/{id}` - видалити файл
   - `GET /api/files/{id}` - отримати інформацію про файл

4. **Налаштувати blob storage**
   - Локальне зберігання для dev (wwwroot/uploads)
   - Azure Blob Storage для production (опціонально)
   - Google Cloud Storage (опціонально)
   - CDN для швидкої доставки

5. **Додати валідацію та безпеку**
   - Дозволені формати: JPG, PNG, WEBP
   - Максимальний розмір: 10MB
   - Virus scanning (опціонально, ClamAV)
   - MIME type validation
   - Rate limiting для запобігання зловживанню

6. **Створити моделі даних**
   - `UploadedFile` entity
   - Властивості: Id, UserId, FileName, FileSize, MimeType, Url, ThumbnailUrl, CreatedAt

## Файли для створення/модифікації

- `src/Calendary.Core/Entities/UploadedFile.cs`
- `src/Calendary.Core/Interfaces/IFileUploadService.cs`
- `src/Calendary.Application/Services/FileUploadService.cs`
- `src/Calendary.API/Controllers/FilesController.cs`
- `src/Calendary.API/DTOs/FileUploadResponse.cs`
- `appsettings.json` - конфігурація storage

## Критерії успіху

- [ ] Можна завантажити зображення через API
- [ ] Зображення автоматично оптимізуються
- [ ] Thumbnails генеруються автоматично
- [ ] Валідація формату та розміру працює
- [ ] Файли зберігаються безпечно
- [ ] URL до файлів повертаються коректно
- [ ] Unit tests для FileUploadService
- [ ] Integration tests для upload endpoint

## Залежності

Немає (незалежна задача)

## Блокується наступні задачі

- Task 13: Drag & Drop потребує upload API

## Технічні деталі

### FileUploadService
```csharp
public interface IFileUploadService
{
    Task<UploadedFile> UploadAsync(
        IFormFile file,
        Guid userId,
        CancellationToken ct = default);

    Task<List<UploadedFile>> UploadMultipleAsync(
        IFormFileCollection files,
        Guid userId,
        CancellationToken ct = default);

    Task DeleteAsync(Guid fileId, Guid userId);
    Task<string> GetUrlAsync(Guid fileId);
}

public class FileUploadService : IFileUploadService
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public async Task<UploadedFile> UploadAsync(
        IFormFile file, Guid userId, CancellationToken ct = default)
    {
        // Валідація
        ValidateFile(file);

        // Генерація унікального імені
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        // Обробка зображення
        using var image = await Image.LoadAsync(file.OpenReadStream(), ct);

        // Resize якщо більше 4000px
        if (image.Width > 4000 || image.Height > 4000)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(4000, 4000),
                Mode = ResizeMode.Max
            }));
        }

        // Збереження original
        var filePath = Path.Combine("wwwroot", "uploads", fileName);
        await image.SaveAsJpegAsync(filePath, new JpegEncoder
        {
            Quality = 90
        }, ct);

        // Генерація thumbnail
        var thumbnailFileName = $"thumb_{fileName}";
        var thumbnailPath = Path.Combine("wwwroot", "uploads", thumbnailFileName);

        image.Mutate(x => x.Resize(300, 300));
        await image.SaveAsJpegAsync(thumbnailPath, ct);

        // Зберігання в БД
        var uploadedFile = new UploadedFile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = file.FileName,
            FileSize = file.Length,
            MimeType = file.ContentType,
            Url = $"/uploads/{fileName}",
            ThumbnailUrl = $"/uploads/{thumbnailFileName}",
            CreatedAt = DateTime.UtcNow
        };

        _context.UploadedFiles.Add(uploadedFile);
        await _context.SaveChangesAsync(ct);

        return uploadedFile;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length > MaxFileSize)
            throw new BadRequestException("File size exceeds 10MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new BadRequestException("Invalid file format");
    }
}
```

### FilesController
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<FileUploadResponse>> UploadFile(
        IFormFile file)
    {
        var userId = User.GetUserId();
        var uploadedFile = await _fileUploadService.UploadAsync(file, userId);

        return Ok(new FileUploadResponse
        {
            Id = uploadedFile.Id,
            Url = uploadedFile.Url,
            ThumbnailUrl = uploadedFile.ThumbnailUrl,
            FileName = uploadedFile.FileName
        });
    }
}
```

### appsettings.json
```json
{
  "FileStorage": {
    "Type": "Local", // or "AzureBlob", "GoogleCloud"
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"],
    "UploadPath": "wwwroot/uploads",
    "CdnUrl": "https://cdn.calendary.com"
  }
}
```

## Примітки

- ImageSharp - безкоштовна бібліотека для обробки зображень
- Для production краще використовувати cloud storage (Azure/GCS)
- CDN значно покращує швидкість завантаження зображень
- Rate limiting важливий для запобігання зловживанню

## Чому Claude?

Складна технічна задача:
- Image processing та optimization
- Security considerations
- Storage abstraction layer
- Потрібне розуміння performance implications

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
