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
4. Генерує 24 стилізованих варіанти → Synthesis + FluxModelService (існує)
5. Користувач обирає 12 найкращих зображень для місяців календаря
6. Створює календар з обраними фото → UserCalendar ✅ (готово)
7. Користувач отримує готовий тематичний календар
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

        // 4. Generate 24 stylized variants (2x the needed amount for selection)
        var synthesisJobs = new List<Job>();
        const int variantsToGenerate = 24; // User will select 12 from these 24
        
        // Generate multiple variants with slight variations
        for (int i = 0; i < variantsToGenerate; i++)
        {
            var photoId = photoIds[i % photoIds.Length]; // Cycle through user photos
            var prompt = BuildPromptForPhoto(templateConfig, photoId, i);
            var job = await _synthesisService.GenerateImageAsync(
                userId,
                prompt,
                templateConfig.FluxPrompt.StyleStrength,
                seed: Random.Shared.Next() // Different seed for variety
            );
            synthesisJobs.Add(job);
        }

        // 5. Store all 24 variants in GeneratedCalendarVariants table
        // 6. User will select 12 favorites via frontend
        // 7. Create final UserCalendar only after user selection
        // 8. Return master job tracking all sub-jobs
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

### 5. Додати Entity для збереження варіантів

**Файл**: `src/Calendary.Model/GeneratedCalendarVariant.cs`

```csharp
public class GeneratedCalendarVariant
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int JobId { get; set; } // Master job that generated these variants
    public int TemplateId { get; set; }
    public int SourcePhotoId { get; set; } // Original user photo
    public int GeneratedFileId { get; set; } // Generated styled image
    public int VariantNumber { get; set; } // 1-24
    public bool IsSelected { get; set; } // User selected this for calendar
    public int? MonthNumber { get; set; } // 1-12 if selected
    public DateTime CreatedAt { get; set; }
    public DateTime? SelectedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Job Job { get; set; } = null!;
    public Template Template { get; set; } = null!;
    public UploadedFile SourcePhoto { get; set; } = null!;
    public UploadedFile GeneratedFile { get; set; } = null!;
}
```

### 6. Додати DTOs

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
    public string Status { get; init; } = "pending"; // pending, training, generating, ready_for_selection, finalizing, completed, failed
    public int Progress { get; init; } // 0-100%
    public string? CurrentStep { get; init; }
    public int GeneratedVariantsCount { get; init; } // 0-24
    public CalendarVariantDto[]? Variants { get; init; } // Available when status = ready_for_selection
    public int? GeneratedCalendarId { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record CalendarVariantDto
{
    public int Id { get; init; }
    public int VariantNumber { get; init; }
    public string ImageUrl { get; init; } = "";
    public string ThumbnailUrl { get; init; } = "";
    public int SourcePhotoId { get; init; }
    public bool IsSelected { get; init; }
    public int? MonthNumber { get; init; }
}

public record SelectCalendarVariantsRequest
{
    public int JobId { get; init; }
    public VariantSelection[] Selections { get; init; } = Array.Empty<VariantSelection>();
}

public record VariantSelection
{
    public int VariantId { get; init; }
    public int MonthNumber { get; init; } // 1-12
}
```

### 7. Додати API endpoints для вибору варіантів

**Файл**: `src/Calendary.Api/Controllers/CalendarsController.cs`

```csharp
/// <summary>
/// Get all 24 generated variants for selection
/// </summary>
[HttpGet("generate/{jobId}/variants")]
public async Task<ActionResult<CalendarVariantDto[]>> GetGeneratedVariants(int jobId)
{
    var currentUser = await CurrentUser;
    if (currentUser == null) return Unauthorized();

    var variants = await _calendarGenerationService.GetVariantsAsync(jobId, currentUser.Id);
    return Ok(variants);
}

/// <summary>
/// Select 12 variants from 24 for final calendar
/// </summary>
[HttpPost("generate/{jobId}/select")]
public async Task<ActionResult<UserCalendarDto>> SelectCalendarVariants(
    int jobId,
    [FromBody] SelectCalendarVariantsRequest request)
{
    var currentUser = await CurrentUser;
    if (currentUser == null) return Unauthorized();

    if (request.Selections.Length != 12)
        return BadRequest("Must select exactly 12 variants (one per month)");

    var calendar = await _calendarGenerationService.FinalizeCalendarAsync(
        jobId,
        currentUser.Id,
        request.Selections
    );

    return Ok(MapToCalendarDto(calendar));
}
```

### 8. Frontend інтеграція

Додати на фронтенді:

1. **Сторінка завантаження фото** (drag & drop)
2. **Вибір шаблону** з preview
3. **Прогрес бар генерації**:
   ```
   [=====>         ] 45%
   Крок 2 з 4: Генерація 24 варіантів зображень...
   ```
4. **Галерея вибору** (NEW!):
   - Показати всі 24 згенеровані варіанти у grid
   - Користувач обирає 12 найкращих
   - Drag & drop для призначення місяців (January → Variant #7)
   - Preview календаря з обраними фото
5. **Можливість regenerate** окремих варіантів
6. **Preview готового календаря** після вибору
7. **Можливість редагування** перед замовленням

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
- Генерація варіантів: ~10-20 хвилин (24 фото × 30-50 сек на фото)
- Використати Job Queue для асинхронної обробки
- WebSocket або SignalR для real-time прогресу
- Паралельна генерація варіантів (до 4 одночасно) для прискорення

### Storage
- Зберігати оригінали та всі 24 згенеровані варіанти
- Видалити невибрані 12 варіантів після фіналізації календаря
- Автоматичне видалення всіх варіантів через 7 днів якщо користувач не зробив вибір
- CDN для швидкої доставки preview та thumbnails

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

1. **Phase 1** (MVP): Генерація 24 варіантів + UI для вибору 12
2. **Phase 2**: Всі 10 тематичних стилів
3. **Phase 3**: Можливість regenerate окремих варіантів
4. **Phase 4**: Кастомні промпти від користувача
5. **Phase 5**: AI рекомендації кращих варіантів на базі якості/естетики

## Metrics для відстеження

- Час генерації 24 варіантів (SLA: < 20 хв)
- Success rate генерації (Target: > 95%)
- Середній час вибору користувачем (insights для UX)
- % користувачів що завершили вибір (Drop-off rate)
- Які варіанти обирають частіше (для оптимізації промптів)
- Користувацька задоволеність (рейтинг результату)
- Conversion rate (варіанти → фінальний календар → purchase)

## Залежності

- Replicate API credits
- Flux model availability
- Storage для тренування та згенерованих файлів

## Ризики

- ⚠️ API rate limits на Replicate (24 requests замість 12)
- ⚠️ Вдвічі вища вартість генерації (24 варіанти = definitely paid feature!)
- ⚠️ Довший час очікування (10-20 хв) може відштовхнути користувачів
- ⚠️ Якість результату може не задовольнити всіх
- ⚠️ Складніший UX з вибором варіантів (може заплутати користувачів)
- ⚠️ Більше storage потрібно для 24 варіантів

## Рішення ризиків

- Показувати realistic ETA (10-20 хв)
- Email/Push notification коли всі 24 варіанти готові
- Progressive loading: показувати варіанти по мірі генерації (не чекати всіх 24)
- Preview mode для швидкого тесту (6 варіантів замість 24, lower quality)
- Простий wizard для вибору з drag & drop
- AI pre-selection: показати "рекомендовані" варіанти першими
- Можливість regenerate окремих варіантів якщо жоден не подобається
- Clear pricing: "24 унікальні варіанти за X₴"

---

**Next Steps**: Обговорити з командою, оцінити effort, додати в sprint planning

**Estimated Effort**: 2-3 тижні (1 senior dev + AI/ML консультант)

**Business Value**: HIGH - це killer feature, що відрізняє від конкурентів! 🚀
