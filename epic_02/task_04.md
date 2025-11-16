# Task 04: API endpoints для шаблонів календарів ✅

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Claude
**Паралельно з**: Task 01, 02, 03, 07
**Завершено**: 2025-11-16

## Опис задачі

Створити backend API endpoints для роботи з шаблонами календарів. Шаблони - це готові дизайни календарів, які користувачі можуть вибрати та персоналізувати.

## Проблема

Користувачі повинні мати доступ до каталогу готових шаблонів календарів з можливістю фільтрації, пошуку, та попереднього перегляду.

## Що треба зробити

### 1. Database Schema

**Створити таблицю `CalendarTemplates`:**
```csharp
public class CalendarTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Business", "Family", "Wedding", etc.
    public decimal BasePrice { get; set; }
    public string PreviewImageUrl { get; set; } = string.Empty;
    public string TemplateDataJson { get; set; } = string.Empty; // JSON з layout даними
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Metadata
    public string[] Tags { get; set; } = Array.Empty<string>(); // для пошуку
    public int PopularityScore { get; set; } = 0; // для сортування
    public bool IsFeatured { get; set; } = false; // виділені шаблони
}
```

**EF Core міграція:**
```bash
dotnet ef migrations add AddCalendarTemplates
dotnet ef database update
```

### 2. Repository Layer

**Створити `ICalendarTemplateRepository`:**
```csharp
public interface ICalendarTemplateRepository
{
    Task<IEnumerable<CalendarTemplate>> GetAllAsync(
        string? category = null,
        string? searchQuery = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "popularity"
    );
    Task<CalendarTemplate?> GetByIdAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<CalendarTemplate>> GetFeaturedAsync(int count = 5);
}
```

**Реалізація з оптимізацією:**
- Пагінація
- Фільтрація по категоріях
- Full-text search по Name, Description, Tags
- Сортування (popularity, price, newest)
- Індекси на Category, IsActive, PopularityScore

### 3. API Endpoints

**TemplatesController.cs:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    // GET /api/templates
    [HttpGet]
    public async Task<ActionResult<PagedResult<TemplateDto>>> GetTemplates(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "popularity"
    )
    {
        // Валідація
        // Виклик сервісу
        // Маппінг до DTO
        // Повернення PagedResult
    }

    // GET /api/templates/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateDetailDto>> GetTemplateById(int id)
    {
        // Отримання з кешу (Redis)
        // Якщо немає - з БД
        // Маппінг до DetailDto
    }

    // GET /api/templates/categories
    [HttpGet("categories")]
    [ResponseCache(Duration = 3600)] // кешувати на 1 годину
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        // Повернути список унікальних категорій
    }

    // GET /api/templates/featured
    [HttpGet("featured")]
    [ResponseCache(Duration = 1800)] // кешувати на 30 хвилин
    public async Task<ActionResult<IEnumerable<TemplateDto>>> GetFeatured()
    {
        // Повернути featured шаблони для головної сторінки
    }
}
```

### 4. DTOs

**TemplateDto.cs (для списку):**
```csharp
public record TemplateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public string PreviewImageUrl { get; init; } = string.Empty;
    public string[] Tags { get; init; } = Array.Empty<string>();
    public bool IsFeatured { get; init; }
}

public record TemplateDetailDto : TemplateDto
{
    public string TemplateDataJson { get; init; } = string.Empty;
    public int PopularityScore { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

### 5. Сервісний шар

**ICalendarTemplateService:**
```csharp
public interface ICalendarTemplateService
{
    Task<PagedResult<TemplateDto>> GetTemplatesAsync(
        string? category,
        string? searchQuery,
        int page,
        int pageSize,
        string sortBy
    );
    Task<TemplateDetailDto?> GetTemplateByIdAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<TemplateDto>> GetFeaturedAsync();
}
```

### 6. Seed даних

**Створити seed для тестових шаблонів:**
```csharp
public static class TemplateSeeder
{
    public static void SeedTemplates(CalendaryDbContext context)
    {
        if (context.CalendarTemplates.Any()) return;

        var templates = new[]
        {
            new CalendarTemplate
            {
                Name = "Modern Business Calendar 2026",
                Description = "Мінімалістичний дизайн для офісу",
                Category = "Business",
                BasePrice = 299.00m,
                PreviewImageUrl = "/templates/business-modern.jpg",
                Tags = new[] { "business", "minimal", "professional" },
                IsFeatured = true,
                PopularityScore = 100
            },
            new CalendarTemplate
            {
                Name = "Family Moments 2026",
                Description = "Календар для сімейних фото",
                Category = "Family",
                BasePrice = 349.00m,
                PreviewImageUrl = "/templates/family-moments.jpg",
                Tags = new[] { "family", "photos", "memories" },
                IsFeatured = true,
                PopularityScore = 95
            },
            // ... додати ще 8-10 шаблонів
        };

        context.CalendarTemplates.AddRange(templates);
        context.SaveChanges();
    }
}
```

### 7. Тестування

**Integration tests:**
```csharp
public class TemplatesControllerTests
{
    [Fact]
    public async Task GetTemplates_ReturnsPagedResult()
    {
        // Arrange
        // Act
        // Assert
    }

    [Fact]
    public async Task GetTemplateById_ValidId_ReturnsTemplate()
    {
        // Arrange
        // Act
        // Assert
    }

    [Fact]
    public async Task GetTemplates_WithCategory_ReturnsFiltered()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

## Файли для створення/модифікації

**Backend (Calendary.Api):**
- `src/Calendary.Model/CalendarTemplate.cs` - модель
- `src/Calendary.Repos/CalendarTemplateRepository.cs` - репозиторій
- `src/Calendary.Core/Services/CalendarTemplateService.cs` - сервіс
- `src/Calendary.Api/Controllers/TemplatesController.cs` - контролер
- `src/Calendary.Api/DTOs/TemplateDto.cs` - DTO
- `src/Calendary.Repos/Seeds/TemplateSeeder.cs` - seed даних
- `src/Calendary.Repos/Migrations/` - EF Core міграція

## Критерії успіху

- [x] Database schema створено та міграція застосована
- [x] Repository layer реалізовано з фільтрацією та пагінацією
- [x] API endpoints працюють:
  - `GET /api/templates` - список з фільтрами
  - `GET /api/templates/{id}` - деталі шаблону
  - `GET /api/templates/categories` - список категорій
  - `GET /api/templates/featured` - featured шаблони
- [x] DTOs створені та маппінг працює
- [x] Seed даних для тестування (мінімум 10 шаблонів)
- [x] Response caching налаштовано
- [x] Swagger документація згенерована
- [x] Integration tests написані та проходять

## Приклад API відповіді

**GET /api/templates?category=Business&page=1&pageSize=10**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Modern Business Calendar 2026",
      "description": "Мінімалістичний дизайн для офісу",
      "category": "Business",
      "basePrice": 299.00,
      "previewImageUrl": "/templates/business-modern.jpg",
      "tags": ["business", "minimal", "professional"],
      "isFeatured": true
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3
}
```

## Залежності

- Task 07: Database schema (можна робити паралельно, але треба координувати міграції)

## Блокує наступні задачі

- Task 09: Сторінка каталогу (потребує ці API endpoints)

## Примітки

### Чому Claude?

Ця задача потребує:
- Архітектурних рішень (Repository pattern, Service layer)
- Оптимізації запитів (індекси, пагінація, кешування)
- Розуміння best practices для API design
- Продумування performance (N+1 queries, eager loading)

Claude краще справляється з такими задачами, ніж Codex.

### Performance considerations

- Додати індекси на `Category`, `IsActive`, `PopularityScore`
- Response caching для categories та featured
- Redis caching для популярних шаблонів
- Пагінація обов'язкова (не віддавати всі шаблони одразу)

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
