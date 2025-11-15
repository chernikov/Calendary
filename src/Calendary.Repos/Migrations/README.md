# Database Migrations

## Поточний стан

**Дата останнього оновлення:** 2025-11-15

Всі попередні міграції були видалені та замінені на одну Initial міграцію, яка відображає поточну схему бази даних.

## Міграції

### InitialCreate (2025-11-15)

Початкова міграція, що містить повну схему бази даних Calendary.

**Таблиці:**
- Users (користувачі)
- Roles (ролі)
- UserRoles (зв'язок користувачів та ролей)
- Languages (мови)
- Countries (країни)
- Categories (категорії для Flux моделей)
- Calendars (календарі)
- CalendarHolidays (зв'язок календарів та свят)
- EventDates (події користувача)
- Holidays (свята)
- FluxModels (AI моделі)
- Trainings (тренування моделей)
- Prompts (промпти для генерації)
- PromptSeeds (seed для промптів)
- PromptThemes (теми промптів)
- Synthesis (згенеровані зображення)
- Images (зображення календарів)
- Photos (фото для тренування)
- Orders (замовлення)
- OrderItems (позиції замовлення)
- PaymentInfos (інформація про оплату)
- MonoWebhookEvents (webhook події від MonoPay)
- WebHooks (webhook для Replicate)
- WebHookFluxModels (зв'язок webhook та моделей)
- VerificationEmailCodes (коди верифікації email)
- VerificationPhoneCodes (коди верифікації телефону)
- ResetTokens (токени скидання паролю)
- UserSettings (налаштування користувача)
- Jobs (фонові задачі)
- JobTasks (підзадачі)

**Seed дані:**
- 2 ролі (Admin, User)
- 2 мови (Українська, English)
- 1 країна (Україна)
- 1 адмін користувач (admin@calendary.com.ua)
- 10 категорій моделей (різні вікові групи та статі)

Детальніше про seed дані: [docs/database-seed-report.md](../../../docs/database-seed-report.md)

## Команди для роботи з міграціями

### Створення нової міграції

```bash
cd src/Calendary.Repos
dotnet ef migrations add MigrationName --startup-project ../Calendary.Api
```

### Застосування міграцій до бази даних

```bash
cd src/Calendary.Repos
dotnet ef database update --startup-project ../Calendary.Api
```

### Видалення останньої міграції

```bash
cd src/Calendary.Repos
dotnet ef migrations remove --startup-project ../Calendary.Api
```

### Генерація SQL скрипту

```bash
cd src/Calendary.Repos
dotnet ef migrations script --startup-project ../Calendary.Api --output migration.sql
```

### Повне пересоздання бази даних (ОБЕРЕЖНО!)

```bash
cd src/Calendary.Repos
dotnet ef database drop --startup-project ../Calendary.Api
dotnet ef database update --startup-project ../Calendary.Api
```

## Структура seed даних

Seed дані винесені в окремий клас `DbSeeder.cs` для кращої організації коду.

```csharp
DbSeeder.SeedData(modelBuilder);
```

Метод `SeedData` викликається з `CalendaryDbContext.OnModelCreating()` і включає:
- `SeedRoles()` - ролі користувачів
- `SeedLanguages()` - мови інтерфейсу
- `SeedCountries()` - країни
- `SeedAdminUser()` - адміністратор за замовчуванням
- `SeedAdminRole()` - призначення ролі Admin
- `SeedCategories()` - категорії моделей

## Важливі примітки

1. **Не змінюйте seed дані вручну в БД** - вони будуть перезаписані при міграціях
2. **Пароль адміна за замовчуванням: `admin`** - обов'язково змініть на продакшені!
3. **Використовуйте міграції для змін схеми** - не змінюйте БД вручну
4. **Backup перед міграцією на продакшені** - завжди створюйте резервну копію

## Connection String

Налаштування в `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=calendary;Username=postgres;Password=your_password"
  }
}
```

Або для MS SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=calendary;User Id=sa;Password=your_password;TrustServerCertificate=True"
  }
}
```

## Історія змін

### 2025-11-15 - Ребазінг міграцій
- Видалено всі попередні міграції (66 файлів)
- Створено нову Initial міграцію на основі поточної схеми
- Винесено seed дані в окремий клас `DbSeeder`
- Створено документацію про seed дані

### Попередня історія
Всі попередні зміни консолідовані в InitialCreate міграцію.
