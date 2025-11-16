# BACKLOG: AI Calendar Generation Integration

## Пріоритет: HIGH
**Epic**: 02 - Customer Portal
**Залежить від**: Tasks 04-08 (completed)
**Створено**: 2025-11-16

## Ідея

Інтегрувати існуючу AI систему (Flux/Replicate) для автоматичної генерації стилізованих календарів на базі фото користувача та обраного тематичного шаблону.

## Концепція Workflow

```
1. Користувач завантажує свої фото → FileUploadService ✅ (готово)
2. Обирає тематичний шаблон → TemplatesController ✅ (готово)
3. Система тренує Flux модель на фото користувача → Training (існує)
4. Генерує 12 стилізованих зображень → Synthesis + FluxModelService (існує)
5. Створює календар з обробленими фото → UserCalendar ✅ (готово)
6. Користувач отримує готовий тематичний календар
```

## Що вже є в системі

### Готова інфраструктура:
- ✅ **FluxModel** - AI моделі для генерації
- ✅ **Training** - тренування моделі на фото (LoRA)
- ✅ **Synthesis** - генерація нових зображень
- ✅ **ReplicateService** - інтеграція з Replicate API
- ✅ **PromptService** - управління промптами
- ✅ **PromptEnhancerService** - покращення промптів
- ✅ **FluxModelService** - сервіс для роботи з Flux
- ✅ **JobService** - асинхронні задачі
- ✅ **FileUploadService** - завантаження фото користувача
- ✅ **Template** - тематичні шаблони

## Що треба зробити

### 1. Розширити Template entity

**Файл**: `src/Calendary.Model/Template.cs`

Додати зв'язок з AI промптами:

```csharp
public class Template
{
    // ... existing fields

    // AI Configuration
    public int? FluxModelId { get; set; }  // Default Flux model for this theme
    public int? PromptThemeId { get; set; } // Theme for prompts

    // Navigation properties
    public FluxModel? FluxModel { get; set; }
    public PromptTheme? PromptTheme { get; set; }
}
```

### 2. Наповнити TemplateData конкретними промптами

**Файл**: `src/Calendary.Repos/DbSeeder.cs`

Замість порожнього `"{}"` додати реальні AI конфігурації:

```csharp
// Сімейний календар
TemplateData = JsonSerializer.Serialize(new {
    theme = "family",
    fluxPrompt = new {
        basePrompt = "family portrait style, warm cozy atmosphere, soft pastel colors, loving family moments",
        negativePrompt = "dark, gloomy, professional, corporate, cold, formal",
        styleStrength = 0.7,
        steps = 30
    },
    layout = new {
        type = "12-month-grid",
        photoProcessing = "apply-family-style",
        monthsPerPage = 1,
        photoFrameStyle = "rounded-warm"
    },
    colorScheme = new[] { "#FFE5E5", "#FFC4C4", "#FFD4D4" }
})

// Корпоративний календар
TemplateData = JsonSerializer.Serialize(new {
    theme = "corporate",
    fluxPrompt = new {
        basePrompt = "professional business portrait, clean minimal aesthetic, sharp focus, corporate environment",
        negativePrompt = "casual, playful, childish, bright colors, cartoonish",
        styleStrength = 0.6,
        steps = 25
    },
    layout = new {
        type = "business-calendar",
        photoProcessing = "professional-portrait",
        includeNotes = true
    },
    colorScheme = new[] { "#1E3A8A", "#374151", "#F3F4F6" }
})

// Весільний календар
TemplateData = JsonSerializer.Serialize(new {
    theme = "wedding",
    fluxPrompt = new {
        basePrompt = "romantic wedding photography, elegant soft lighting, white and gold accents, dreamy atmosphere",
        negativePrompt = "dark, harsh lighting, casual, everyday",
        styleStrength = 0.8,
        steps = 35
    },
    layout = new {
        type = "romantic-grid",
        photoProcessing = "wedding-style",
        decorativeElements = "roses-hearts"
    },
    colorScheme = new[] { "#FFFFFF", "#FFD700", "#FFC0CB" }
})

// ... інші теми
```

### 3. Створити CalendarGenerationService

**Файл**: `src/Calendary.Core/Services/CalendarGenerationService.cs`

```csharp
public interface ICalendarGenerationService
{
    Task<Job> GenerateCalendarAsync(int userId, int templateId, int[] photoIds);
    Task<CalendarGenerationStatus> GetGenerationStatusAsync(int jobId);
    Task<UserCalendar> GetGeneratedCalendarAsync(int jobId);
}

public class CalendarGenerationService : ICalendarGenerationService
{
    private readonly ITrainingService _trainingService;
    private readonly ISynthesisService _synthesisService;
    private readonly IFluxModelService _fluxModelService;
    private readonly ITemplateRepository _templateRepository;
    private readonly IUploadedFileRepository _fileRepository;
    private readonly IUserCalendarRepository _calendarRepository;
    private readonly IJobService _jobService;

    public async Task<Job> GenerateCalendarAsync(int userId, int templateId, int[] photoIds)
    {
        // 1. Get template with AI configuration
        var template = await _templateRepository.GetByIdAsync(templateId);
        var templateConfig = JsonSerializer.Deserialize<TemplateConfig>(template.TemplateData);

        // 2. Create training job for user's photos
        var trainingJob = await _trainingService.CreateTrainingJobAsync(userId, photoIds);

        // 3. Wait for training completion (or queue next steps)

        // 4. Generate 12 stylized images (one per month)
        var synthesisJobs = new List<Job>();
        foreach (var photoId in photoIds.Take(12))
        {
            var prompt = BuildPromptForPhoto(templateConfig, photoId);
            var job = await _synthesisService.GenerateImageAsync(
                userId,
                prompt,
                templateConfig.FluxPrompt.StyleStrength
            );
            synthesisJobs.Add(job);
        }

        // 5. Create UserCalendar with generated images
        // 6. Return master job tracking all sub-jobs
    }

    private string BuildPromptForPhoto(TemplateConfig config, int photoId)
    {
        return $"{config.FluxPrompt.BasePrompt}, professional calendar photo";
    }
}
```

### 4. Додати API endpoint для генерації

**Файл**: `src/Calendary.Api/Controllers/CalendarsController.cs`

```csharp
/// <summary>
/// Generate AI-styled calendar from user photos
/// </summary>
[HttpPost("generate")]
public async Task<ActionResult<JobDto>> GenerateCalendar(
    [FromBody] GenerateCalendarRequest request)
{
    var currentUser = await CurrentUser;
    if (currentUser == null) return Unauthorized();

    // Validate user owns all photos
    foreach (var photoId in request.PhotoIds)
    {
        var file = await _fileRepository.GetByIdAndUserIdAsync(photoId, currentUser.Id);
        if (file == null) return BadRequest($"Photo {photoId} not found");
    }

    var job = await _calendarGenerationService.GenerateCalendarAsync(
        currentUser.Id,
        request.TemplateId,
        request.PhotoIds
    );

    return Ok(MapToJobDto(job));
}

/// <summary>
/// Get calendar generation status
/// </summary>
[HttpGet("generate/{jobId}/status")]
public async Task<ActionResult<CalendarGenerationStatusDto>> GetGenerationStatus(int jobId)
{
    var status = await _calendarGenerationService.GetGenerationStatusAsync(jobId);
    return Ok(status);
}
```

### 5. Додати DTOs

**Файл**: `src/Calendary.Api/Dtos/CalendarGenerationDto.cs`

```csharp
public record GenerateCalendarRequest
{
    public int TemplateId { get; init; }
    public int[] PhotoIds { get; init; } = Array.Empty<int>();
}

public record CalendarGenerationStatusDto
{
    public int JobId { get; init; }
    public string Status { get; init; } = "pending"; // pending, training, generating, completed, failed
    public int Progress { get; init; } // 0-100%
    public string? CurrentStep { get; init; }
    public int? GeneratedCalendarId { get; init; }
    public DateTime? CompletedAt { get; init; }
}
```

### 6. Frontend інтеграція

Додати на фронтенді:

1. **Сторінка завантаження фото** (drag & drop)
2. **Вибір шаблону** з preview
3. **Прогрес бар генерації**:
   ```
   [=====>         ] 45%
   Крок 2 з 4: Генерація стилізованих фото...
   ```
4. **Preview готового календаря**
5. **Можливість редагування** перед замовленням

## Приклади промптів для кожної теми

### Сімейний (Family)
```
Base: "family portrait, warm cozy home atmosphere, soft pastel colors, loving moments, gentle lighting"
Negative: "dark, gloomy, professional, corporate, cold, formal, harsh shadows"
Style strength: 0.7
```

### Корпоративний (Corporate)
```
Base: "professional business portrait, clean minimalist aesthetic, sharp focus, modern office environment"
Negative: "casual, playful, childish, bright colors, cartoonish, messy"
Style strength: 0.6
```

### Весільний (Wedding)
```
Base: "romantic wedding photography, elegant soft lighting, white and gold accents, dreamy atmosphere, love and joy"
Negative: "dark, harsh lighting, casual, everyday, simple"
Style strength: 0.8
```

### Спортивний (Sports)
```
Base: "dynamic sports photography, energetic action, bold colors, athletic performance, motion blur"
Negative: "static, boring, dull, passive, slow"
Style strength: 0.75
```

### Дитячий (Kids)
```
Base: "playful children photography, bright cheerful colors, fun and joy, cartoon-like elements, happy moments"
Negative: "serious, formal, dark, adult, professional"
Style strength: 0.85
```

### Мінімалістичний (Minimalist)
```
Base: "minimalist clean photography, simple elegant composition, monochrome or muted colors, zen aesthetic"
Negative: "cluttered, busy, bright colors, decorative, complex"
Style strength: 0.5
```

### Природа (Nature)
```
Base: "natural outdoor photography, landscape integration, earthy tones, organic elements, environmental portrait"
Negative: "urban, indoor, artificial, man-made, city"
Style strength: 0.7
```

### Подорожі (Travel)
```
Base: "travel adventure photography, exotic locations, wanderlust aesthetic, cultural elements, journey moments"
Negative: "home, indoor, boring, local, everyday"
Style strength: 0.75
```

### Вінтаж (Vintage)
```
Base: "vintage retro photography, film grain, sepia tones, nostalgic atmosphere, classic timeless style"
Negative: "modern, digital, sharp, contemporary, futuristic"
Style strength: 0.8
```

### Професійний (Professional)
```
Base: "professional executive portrait, confident business look, premium quality, leadership presence"
Negative: "casual, informal, amateur, low quality, messy"
Style strength: 0.65
```

## Технічні вимоги

### Performance
- Генерація календаря: ~5-10 хвилин (12 фото × 30-50 сек на фото)
- Використати Job Queue для асинхронної обробки
- WebSocket або SignalR для real-time прогресу

### Storage
- Зберігати оригінали та згенеровані версії
- Автоматичне видалення через 30 днів після створення замовлення
- CDN для швидкої доставки preview

### Cost Optimization
- Кешувати згенеровані стилі для однакових комбінацій шаблон+фото
- Batch processing для зниження вартості API calls
- Опціональний preview mode (lower quality, faster, cheaper)

## Міграція даних

```sql
-- Add columns to Templates table
ALTER TABLE Templates
ADD FluxModelId INT NULL,
    PromptThemeId INT NULL;

-- Add foreign keys
ALTER TABLE Templates
ADD CONSTRAINT FK_Templates_FluxModels
    FOREIGN KEY (FluxModelId) REFERENCES FluxModels(Id);

ALTER TABLE Templates
ADD CONSTRAINT FK_Templates_PromptThemes
    FOREIGN KEY (PromptThemeId) REFERENCES PromptThemes(Id);
```

## Пріоритезація

1. **Phase 1** (MVP): Базова генерація з 1 стилем
2. **Phase 2**: Всі 10 тематичних стилів
3. **Phase 3**: Кастомні промпти від користувача
4. **Phase 4**: AI рекомендації стилю на базі фото

## Metrics для відстеження

- Час генерації календаря (SLA: < 10 хв)
- Success rate генерації (Target: > 95%)
- Користувацька задоволеність (рейтинг результату)
- Conversion rate (preview → purchase)

## Залежності

- Replicate API credits
- Flux model availability
- Storage для тренування та згенерованих файлів

## Ризики

- ⚠️ API rate limits на Replicate
- ⚠️ Висока вартість генерації (paid feature?)
- ⚠️ Довгий час очікування може відштовхнути користувачів
- ⚠️ Якість результату може не задовольнити всіх

## Рішення ризиків

- Показувати realistic ETA (5-10 хв)
- Email notification коли готово
- Preview mode для швидкого тесту
- Можливість regenerate окремих місяців

---

**Next Steps**: Обговорити з командою, оцінити effort, додати в sprint planning

**Estimated Effort**: 2-3 тижні (1 senior dev + AI/ML консультант)

**Business Value**: HIGH - це killer feature, що відрізняє від конкурентів! 🚀
