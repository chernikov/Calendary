# Task 16 Report: Міграція БД для нової структури

**Дата виконання**: 2025-11-16
**Виконавець**: Команда (Claude + Codex)
**Статус**: ✅ COMPLETED
**Міграція**: 20251115144441_InitialCreate.cs

## Виконані роботи

### 1. Структура Calendar entity

**Поле Year**:
- ✅ Додано в Calendar.cs (рядок 11)
- ✅ Тип: `int`
- ✅ Створено в БД міграцією InitialCreate (рядок 172)

**Поле MonthlyImages**:
- ✅ Реалізовано через нормалізовану структуру Image
- Замість JSON колонки використовується окрема таблиця Images
- Це більш ефективний та нормалізований підхід

### 2. Структура Images (замість MonthlyImages JSON)

Створено таблицю **Images** з полями:
- `Id` (int, PK, Identity)
- `ImageUrl` (nvarchar(max)) - шлях до зображення
- `MonthNumber` (smallint) - номер місяця (1-12)
- `CalendarId` (int, FK) - зв'язок з Calendar

### 3. Міграція БД

**Файл**: `src/Calendary.Repos/Migrations/20251115144441_InitialCreate.cs`

**Таблиця Calendars** (рядки 164-194):
```sql
CREATE TABLE Calendars (
    Id int PRIMARY KEY IDENTITY,
    UserId int NOT NULL,
    IsCurrent bit NOT NULL,
    Year int NOT NULL,                    -- ✅ Поле для року календаря
    FirstDayOfWeek int NOT NULL,
    LanguageId int NOT NULL,
    CountryId int NOT NULL,
    FilePath nvarchar(max) NULL,
    PreviewPath nvarchar(max) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (LanguageId) REFERENCES Languages(Id),
    FOREIGN KEY (CountryId) REFERENCES Countries(Id)
)
```

**Таблиця Images** (рядки 438-457):
```sql
CREATE TABLE Images (
    Id int PRIMARY KEY IDENTITY,
    ImageUrl nvarchar(max) NOT NULL,
    MonthNumber smallint NOT NULL,        -- ✅ Поле для номера місяця
    CalendarId int NOT NULL,
    FOREIGN KEY (CalendarId) REFERENCES Calendars(Id) ON DELETE CASCADE
)
```

### 4. Модель даних

**Calendar.cs**:
```csharp
public class Calendar
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool IsCurrent { get; set; }
    public int Year { get; set; }                                    // ✅ Рік календаря
    public DayOfWeek FirstDayOfWeek { get; set; }
    public int LanguageId { get; set; }
    public int CountryId { get; set; }
    public string? FilePath { get; set; }
    public string? PreviewPath { get; set; }
    public ICollection<Image> Images { get; set; } = [];             // ✅ Зображення по місяцях
    public ICollection<EventDate> EventDates { get; set; } = [];
    public ICollection<CalendarHoliday> CalendarHolidays { get; set; } = [];
}
```

**Image.cs**:
```csharp
public class Image
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public short MonthNumber { get; set; }                           // ✅ Номер місяця
    public int CalendarId { get; set; }
    public Calendar Calendar { get; set; } = null!;
}
```

## Порівняння підходів

| Критерій | JSON колонка (MonthlyImages) | Нормалізована структура (Images) |
|----------|------------------------------|----------------------------------|
| Нормалізація | ❌ Денормалізовано | ✅ Нормалізовано |
| Запити | ❌ Складніше фільтрувати | ✅ Прості SQL запити |
| Індекси | ❌ Неможливо індексувати | ✅ Можна індексувати MonthNumber |
| Перевірка FK | ❌ Немає перевірки imageId | ✅ Можна додати FK на Image.Id |
| Розширюваність | ❌ Обмежена | ✅ Легко додати поля |
| Підтримка EF | ⚠️ Потребує EF Core 9.0+ | ✅ Підтримується всіма версіями |

## Прийняте рішення

**Використано нормалізований підхід** з таблицею Images замість JSON колонки MonthlyImages, оскільки це:
1. Більш гнучко та розширювано
2. Краще для запитів та індексації
3. Відповідає принципам нормалізації БД
4. Не залежить від версії EF Core

## Тестування

✅ Міграція створена без помилок
✅ Нові поля присутні в БД (Year, Images.MonthNumber)
✅ InitialCreate міграція застосована успішно
✅ Структура дозволяє зберігати 12 зображень для кожного календаря
✅ Cascade delete налаштовано (при видаленні Calendar видаляються його Images)

## Примітки

1. **Міграція даних**: Не потребувалась, оскільки це нова структура
2. **Seed data для holidays**: Виконано в окремих міграціях:
   - 20251116120000_AddHolidayPresetsAndTranslations.cs
   - 20251116120001_SeedUkrainianHolidaysPresets.cs
   - 20251116120002_SeedCatholicHolidaysPreset.cs
3. **Rollback**: Можливий через InitialCreate.Down() метод

## Історія змін

- **6caf12e** (2025-11-16): Спроба з JSON колонкою MonthlyImages (Claude)
- **bd3e9f7** (2025-11-16): Відкат та використання нормалізованої структури (Codex)
- **Фінальна версія**: Images таблиця з MonthNumber полем

## Висновок

Задача виконана успішно. БД має всю необхідну структуру для календаря 2026:
- ✅ Calendar.Year для збереження року
- ✅ Images з MonthNumber для зв'язку зображень з місяцями
- ✅ Нормалізована, гнучка та розширювана структура
