# Task 07: Database schema для Customer Portal

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude
**Паралельно з**: Task 01, 02, 03, 04, 05, 06

## Опис задачі

Спроектувати та створити database schema для Customer Portal: Templates, UserCalendars, CartItems, Orders. Створити EF Core міграції.

## Проблема

Потрібна структурована база даних для зберігання шаблонів, календарів користувачів, кошика та замовлень.

## Що треба зробити

1. **Створити нові entities**
   - `Template` - шаблони календарів
   - `UserCalendar` - календарі користувачів
   - `CartItem` - елементи кошика
   - `UploadedFile` - завантажені файли

2. **Розширити існуючі entities**
   - `Order` - додати поля для Customer Portal
   - `User` - додати профільні дані

3. **Template Entity**
   ```csharp
   - Id (Guid, PK)
   - Name (string, required)
   - Description (string)
   - Category (string) - "Сімейний", "Корпоративний", "Спортивний"
   - PreviewImageUrl (string)
   - TemplateData (JSON) - default canvas state
   - Price (decimal)
   - IsActive (bool)
   - SortOrder (int)
   - CreatedAt, UpdatedAt
   ```

4. **UserCalendar Entity**
   ```csharp
   - Id (Guid, PK)
   - UserId (Guid, FK)
   - TemplateId (Guid?, FK nullable)
   - Title (string)
   - DesignData (JSON) - canvas state
   - PreviewImageUrl (string)
   - Status (enum: Draft, Completed)
   - CreatedAt, UpdatedAt
   - IsDeleted (bool)
   ```

5. **CartItem Entity**
   ```csharp
   - Id (Guid, PK)
   - UserId (Guid, FK)
   - CalendarId (Guid, FK)
   - Quantity (int)
   - Format (enum: A3, A4)
   - PaperType (enum: Glossy, Matte)
   - Price (decimal)
   - CreatedAt, UpdatedAt
   ```

6. **Створити індекси**
   - `Template`: Index on Category, IsActive
   - `UserCalendar`: Index on UserId, Status, IsDeleted
   - `CartItem`: Index on UserId
   - `Order`: Index on UserId, Status

7. **Створити EF Core міграцію**
   ```bash
   dotnet ef migrations add AddCustomerPortalTables
   ```

8. **Seed data для Templates**
   - Створити 10-15 базових шаблонів
   - Різні категорії
   - Preview images

## Файли для створення/модифікації

- `src/Calendary.Core/Entities/Template.cs`
- `src/Calendary.Core/Entities/UserCalendar.cs`
- `src/Calendary.Core/Entities/CartItem.cs`
- `src/Calendary.Core/Entities/UploadedFile.cs`
- `src/Calendary.Core/Enums/CalendarStatus.cs`
- `src/Calendary.Core/Enums/PaperFormat.cs`
- `src/Calendary.Core/Enums/PaperType.cs`
- `src/Calendary.Infrastructure/Data/ApplicationDbContext.cs`
- `src/Calendary.Infrastructure/Data/Configurations/TemplateConfiguration.cs`
- `src/Calendary.Infrastructure/Data/Configurations/UserCalendarConfiguration.cs`
- `src/Calendary.Infrastructure/Data/Configurations/CartItemConfiguration.cs`
- `src/Calendary.Infrastructure/Migrations/` - нова міграція

## Критерії успіху

- [ ] Всі entities створені
- [ ] EF Core configurations налаштовані
- [ ] Індекси створені
- [ ] Міграція створена та застосована
- [ ] Seed data для Templates створена
- [ ] Foreign keys налаштовані коректно
- [ ] Database diagram оновлена

## Залежності

Немає (незалежна задача)

## Блокується наступні задачі

- Task 04: API для templates потребує БД
- Task 05: API для calendars потребує БД
- Task 17: Кошик потребує CartItem таблицю

## Технічні деталі

### Template Entity
```csharp
public class Template
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PreviewImageUrl { get; set; } = string.Empty;
    public string TemplateData { get; set; } = "{}"; // JSON
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<UserCalendar> UserCalendars { get; set; } = new List<UserCalendar>();
}
```

### TemplateConfiguration
```csharp
public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Price)
            .HasPrecision(18, 2);

        builder.Property(t => t.TemplateData)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(t => new { t.Category, t.IsActive });
        builder.HasIndex(t => t.SortOrder);
    }
}
```

### Seed Data
```csharp
public static class TemplateSeedData
{
    public static List<Template> GetTemplates()
    {
        return new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Сімейний календар 2025",
                Description = "Класичний сімейний календар з місяцями",
                Category = "Сімейний",
                PreviewImageUrl = "/templates/family-2025.jpg",
                TemplateData = "{}",
                Price = 299.00m,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // ... більше шаблонів
        };
    }
}
```

## Примітки

- JSON columns для flexible data (DesignData, TemplateData)
- Soft delete для UserCalendar (IsDeleted)
- Decimal precision для грошових значень
- Індекси для performance оптимізації

## Чому Claude?

Архітектурна задача:
- Database design decisions
- EF Core configuration
- Index optimization
- Потрібне розуміння data relationships

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
