# Entity Type Configurations

Ця папка містить конфігурації для всіх Entity Framework моделей проекту Calendary.

## Структура

Кожна модель має свій окремий файл конфігурації, що реалізує `IEntityTypeConfiguration<T>`:

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Configuration logic
    }
}
```

## Автоматичне застосування

Всі конфігурації автоматично застосовуються в `CalendaryDbContext` через:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalendaryDbContext).Assembly);
```

## Конфігурації зі складною логікою

### Composite Keys

**UserRoleConfiguration.cs** - Many-to-Many зв'язок між Users та Roles
```csharp
builder.HasKey(ur => new { ur.UserId, ur.RoleId });
```

**CalendarHolidayConfiguration.cs** - Many-to-Many зв'язок між Calendars та Holidays
```csharp
builder.HasKey(ch => new { ch.CalendarId, ch.HolidayId });
```

### Relationships

**EventDateConfiguration.cs** - UserSetting → EventDates
```csharp
builder.HasOne(ed => ed.UserSetting)
    .WithMany(us => us.EventDates)
    .HasForeignKey(ed => ed.UserSettingId);
```

**HolidayConfiguration.cs** - Country → Holidays
```csharp
builder.HasOne(h => h.Country)
    .WithMany(c => c.Holidays)
    .HasForeignKey(h => h.CountryId);
```

### Cascade Delete Restrictions (SQL Server)

**JobConfiguration.cs** - User → Jobs
```csharp
builder.HasOne(j => j.User)
    .WithMany()
    .HasForeignKey(j => j.UserId)
    .OnDelete(DeleteBehavior.Restrict);
```

**JobTaskConfiguration.cs** - Job → JobTasks
```csharp
builder.HasOne(jt => jt.Job)
    .WithMany(j => j.Tasks)
    .HasForeignKey(jt => jt.JobId)
    .OnDelete(DeleteBehavior.Restrict);
```

### Column Types

**OrderItemConfiguration.cs** - Decimal precision
```csharp
builder.Property(o => o.Price)
    .HasColumnType("decimal(18, 2)");
```

## Список всіх конфігурацій (30)

### Core Entities
1. **CalendarConfiguration.cs** - Календарі користувачів
2. **CategoryConfiguration.cs** - Категорії Flux моделей
3. **CountryConfiguration.cs** - Країни для свят
4. **ImageConfiguration.cs** - Зображення календарів
5. **LanguageConfiguration.cs** - Мови інтерфейсу
6. **OrderConfiguration.cs** - Замовлення
7. **OrderItemConfiguration.cs** - Позиції замовлення (decimal price)
8. **PaymentInfoConfiguration.cs** - Інформація про оплату
9. **RoleConfiguration.cs** - Ролі користувачів
10. **UserConfiguration.cs** - Користувачі

### Relationships
11. **CalendarHolidayConfiguration.cs** - Зв'язок календарів та свят (composite key)
12. **EventDateConfiguration.cs** - Важливі дати користувачів
13. **HolidayConfiguration.cs** - Свята країн
14. **UserRoleConfiguration.cs** - Зв'язок користувачів та ролей (composite key)
15. **UserSettingConfiguration.cs** - Налаштування користувачів

### AI/Replicate
16. **FluxModelConfiguration.cs** - AI моделі користувачів
17. **PhotoConfiguration.cs** - Фото для тренування
18. **PromptConfiguration.cs** - Промпти для генерації
19. **PromptSeedConfiguration.cs** - Seeds для промптів
20. **PromptThemeConfiguration.cs** - Теми промптів
21. **SynthesisConfiguration.cs** - Згенеровані зображення
22. **TrainingConfiguration.cs** - Тренування моделей
23. **WebHookConfiguration.cs** - Webhooks від Replicate
24. **WebHookFluxModelConfiguration.cs** - Зв'язок webhooks та моделей

### Background Jobs
25. **JobConfiguration.cs** - Фонові задачі (cascade restrict)
26. **JobTaskConfiguration.cs** - Підзадачі (cascade restrict)

### Authentication & Security
27. **ResetTokenConfiguration.cs** - Токени скидання паролю
28. **VerificationEmailCodeConfiguration.cs** - Коди верифікації email
29. **VerificationPhoneCodeConfiguration.cs** - Коди верифікації телефону

### Payment
30. **MonoWebhookEventConfiguration.cs** - Webhook події MonoPay

## Додавання нової конфігурації

1. Створіть новий файл `{EntityName}Configuration.cs`
2. Реалізуйте `IEntityTypeConfiguration<{EntityName}>`
3. Конфігурація буде автоматично застосована через `ApplyConfigurationsFromAssembly`

Приклад:

```csharp
using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
{
    public void Configure(EntityTypeBuilder<NewEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(e => e.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(e => e.ParentId);

        // Indexes
        builder.HasIndex(e => e.Code)
            .IsUnique();
    }
}
```

## Переваги цього підходу

✅ **Separation of Concerns** - Кожна модель має свою конфігурацію
✅ **Легкість підтримки** - Зміни локалізовані в одному файлі
✅ **Читабельність** - `CalendaryDbContext` не захаращений конфігурацією
✅ **Тестування** - Легко тестувати окремі конфігурації
✅ **Масштабованість** - Просто додавати нові моделі

---

**Останнє оновлення:** 2025-11-15
**Кількість конфігурацій:** 30
