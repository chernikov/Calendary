# Task 05: API для управління календарями користувача

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Claude
**Паралельно з**: Task 01, 02, 03, 07

## Опис задачі

Створити CRUD API endpoints для управління календарями користувачів: створення, редагування, збереження дизайнів, видалення.

## Проблема

Користувачі повинні мати можливість створювати, зберігати та редагувати свої календарі через API.

## Що треба зробити

1. **Створити моделі даних**
   - `UserCalendar` model (EF Core)
   - Властивості:
     - Id (Guid)
     - UserId (Guid, FK)
     - TemplateId (Guid, FK, nullable)
     - Title (string)
     - DesignData (JSON string - canvas state)
     - PreviewImageUrl (string)
     - Status (enum: Draft, Completed)
     - CreatedAt, UpdatedAt
     - IsDeleted (soft delete)

2. **Створити API endpoints**
   - `POST /api/calendars` - створити новий календар
   - `GET /api/calendars` - отримати календарі користувача
   - `GET /api/calendars/{id}` - отримати конкретний календар
   - `PUT /api/calendars/{id}` - оновити календар
   - `DELETE /api/calendars/{id}` - видалити календар (soft delete)
   - `POST /api/calendars/{id}/duplicate` - дублювати календар

3. **Імплементувати CalendarController**
   - Authorization (JWT)
   - Валідація входів (FluentValidation)
   - Перевірка, що календар належить користувачу
   - Pagination для списку календарів

4. **Імплементувати CalendarService**
   - Бізнес-логіка для CRUD операцій
   - Автоматичне збереження preview image
   - Auto-save функціонал
   - Копіювання календаря з шаблону

5. **Імплементувати CalendarRepository**
   - Data access layer
   - Запити до БД через EF Core
   - Include User та Template
   - Фільтрація та сортування

6. **Додати валідацію**
   - DesignData повинен бути валідний JSON
   - Title обов'язковий
   - UserId повинен відповідати поточному користувачу

## Файли для створення/модифікації

- `src/Calendary.Core/Entities/UserCalendar.cs`
- `src/Calendary.Core/Enums/CalendarStatus.cs`
- `src/Calendary.Core/Interfaces/ICalendarRepository.cs`
- `src/Calendary.Core/Interfaces/ICalendarService.cs`
- `src/Calendary.Infrastructure/Repositories/CalendarRepository.cs`
- `src/Calendary.Application/Services/CalendarService.cs`
- `src/Calendary.API/Controllers/CalendarsController.cs`
- `src/Calendary.API/DTOs/CreateCalendarRequest.cs`
- `src/Calendary.API/DTOs/UpdateCalendarRequest.cs`
- `src/Calendary.API/DTOs/CalendarResponse.cs`

## Критерії успіху

- [ ] Всі CRUD endpoints працюють
- [ ] Валідація входів працює коректно
- [ ] Користувач може бачити тільки свої календарі
- [ ] DesignData зберігається та читається як JSON
- [ ] Soft delete працює (календарі не видаляються фізично)
- [ ] Unit tests написані для сервісу
- [ ] Swagger документація оновлена

## Залежності

- Task 07: Database schema повинна бути готова

## Блокується наступні задачі

- Task 12: Редактор потребує API для збереження
- Task 16: Preview потребує API для отримання календарів

## Технічні деталі

### UserCalendar Entity
```csharp
public class UserCalendar
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DesignData { get; set; } = "{}"; // JSON
    public string? PreviewImageUrl { get; set; }
    public CalendarStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; } = null!;
    public Template? Template { get; set; }
}
```

### CalendarsController
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarsController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    [HttpPost]
    public async Task<ActionResult<CalendarResponse>> CreateCalendar(
        [FromBody] CreateCalendarRequest request)
    {
        var userId = User.GetUserId();
        var calendar = await _calendarService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetCalendar),
            new { id = calendar.Id }, calendar);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CalendarResponse>> GetCalendar(Guid id)
    {
        var userId = User.GetUserId();
        var calendar = await _calendarService.GetByIdAsync(id, userId);
        return Ok(calendar);
    }

    // ... інші endpoints
}
```

## Примітки

- DesignData - це JSON з canvas state (Fabric.js/Konva.js)
- Preview image генерується на фронтенді та відправляється на бекенд
- Auto-save можна імплементувати через debounced PUT запити

## Чому Claude?

Складна бізнес-логіка:
- CRUD з валідацією та авторизацією
- JSON serialization/deserialization
- Repository pattern implementation
- Потрібне глибоке розуміння архітектури

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
