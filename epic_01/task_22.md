# Task 22: Seed даних для пресетів свят

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Gemini
**Виконано**: 16.11.2025

## Опис задачі

Створити seed даних для системи пресетів свят (Task 21). Наповнити базу даних готовими наборами свят для різних країн та релігій з підтримкою двомовної локалізації (UA/EN).

## Що треба зробити

1. **Створити міграцію з seed даними** для таблиць:
   - `HolidayPresets` - набори свят
   - `HolidayPresetTranslations` - переклади назв пресетів
   - `Holidays` - конкретні свята
   - `HolidayTranslations` - переклади назв свят

2. **Українські державні свята (UA_STATE)**:
   ```csharp
   new HolidayPreset { 
     Code = "UA_STATE", 
     Type = "State",
     Translations = [
       { LanguageCode = "ua", Name = "Українські державні свята" },
       { LanguageCode = "en", Name = "Ukrainian Public Holidays" }
     ],
     Holidays = [
       { Month = 1, Day = 1, Translations = [...] }, // Новий рік
       { Month = 1, Day = 7, Translations = [...] }, // Різдво Orthodox
       // ... інші свята
     ]
   }
   ```

3. **США державні свята (US_STATE)**:
   - New Year's Day (1 січня)
   - Martin Luther King Jr. Day (3-й понеділок січня, рухоме)
   - Presidents' Day (3-й понеділок лютого, рухоме)
   - Memorial Day (останній понеділок травня, рухоме)
   - Independence Day (4 липня)
   - Labor Day (1-й понеділок вересня, рухоме)
   - Columbus Day (2-й понеділок жовтня, рухоме)
   - Veterans Day (11 листопада)
   - Thanksgiving (4-й четвер листопада, рухоме)
   - Christmas (25 грудня)

4. **Великобританія (UK_STATE)**:
   - New Year's Day, Good Friday, Easter Monday
   - Early May Bank Holiday, Spring Bank Holiday, Summer Bank Holiday
   - Christmas Day, Boxing Day

5. **Православні церковні (ORTHODOX)**:
   - Різдво (7 січня)
   - Водохреще (19 січня)
   - Великдень (рухоме, розрахунок за юліанським календарем)
   - Вознесіння (40-й день після Великодня)
   - Трійця (50-й день після Великодня)
   - Успіння (28 серпня)
   - День Святого Миколая (19 грудня)

6. **Католицькі церковні (CATHOLIC)**:
   - Великдень (рухоме, за григоріанським календарем)
   - Попільна середа, Вознесіння, П'ятдесятниця
   - Внебовзяття Діви Марії (15 серпня)
   - День всіх святих (1 листопада)
   - Різдво (25 грудня)

7. **Алгоритми розрахунку рухомих свят**:
   - Easter calculator (Computus algorithm) для православного календаря
   - Easter calculator для католицького календаря (григоріанський)
   - Розрахунок свят, що залежать від Easter (Вознесіння, Трійця тощо)

8. **Міжнародні популярні свята (INTERNATIONAL)** - опціонально:
   - День Святого Валентина (14 лютого)
   - День матері (2-а неділя травня)
   - Хелловін (31 жовтня)
   - День батька (3-я неділя червня)

## Структура таблиць БД

```sql
-- Пресети свят
CREATE TABLE HolidayPresets (
    Id INT PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Type VARCHAR(20) -- 'State', 'Religious', 'International'
);

-- Переклади пресетів
CREATE TABLE HolidayPresetTranslations (
    Id INT PRIMARY KEY,
    PresetId INT FOREIGN KEY,
    LanguageCode VARCHAR(2), -- 'ua', 'en'
    Name NVARCHAR(200),
    Description NVARCHAR(500)
);

-- Свята
CREATE TABLE Holidays (
    Id INT PRIMARY KEY,
    PresetId INT FOREIGN KEY,
    Month INT,
    Day INT,
    IsMovable BIT, -- TRUE для рухомих свят
    CalculationType VARCHAR(50) -- 'Easter_Orthodox', 'Easter_Catholic', 'NthWeekday' тощо
);

-- Переклади свят
CREATE TABLE HolidayTranslations (
    Id INT PRIMARY KEY,
    HolidayId INT FOREIGN KEY,
    LanguageCode VARCHAR(2),
    Name NVARCHAR(200)
);
```

## Файли для створення

- `src/Calendary.Repos/Migrations/XXXXX_CreateHolidayPresetTables.cs`
- `src/Calendary.Repos/Migrations/XXXXX_SeedHolidayPresets.cs`
- `src/Calendary.Core/Services/EasterCalculator.cs` - розрахунок дати Великодня
- `src/Calendary.Core/Services/MovableHolidayCalculator.cs` - розрахунок рухомих свят

## Приклад seed коду

```csharp
// В міграції
protected override void Up(MigrationBuilder migrationBuilder)
{
    // UA_STATE preset
    migrationBuilder.InsertData(
        table: "HolidayPresets",
        columns: new[] { "Id", "Code", "Type" },
        values: new object[] { 1, "UA_STATE", "State" }
    );
    
    // Переклади пресету
    migrationBuilder.InsertData(
        table: "HolidayPresetTranslations",
        columns: new[] { "Id", "PresetId", "LanguageCode", "Name", "Description" },
        values: new object[,] {
            { 1, 1, "ua", "Українські державні свята", "Офіційні державні свята України" },
            { 2, 1, "en", "Ukrainian Public Holidays", "Official public holidays of Ukraine" }
        }
    );
    
    // Свята
    migrationBuilder.InsertData(
        table: "Holidays",
        columns: new[] { "Id", "PresetId", "Month", "Day", "IsMovable" },
        values: new object[,] {
            { 1, 1, 1, 1, false },  // Новий рік
            { 2, 1, 1, 7, false },  // Різдво Orthodox
            { 3, 1, 3, 8, false },  // Жіночий день
            // ...
        }
    );
    
    // Переклади свят
    migrationBuilder.InsertData(
        table: "HolidayTranslations",
        columns: new[] { "Id", "HolidayId", "LanguageCode", "Name" },
        values: new object[,] {
            { 1, 1, "ua", "Новий рік" },
            { 2, 1, "en", "New Year's Day" },
            { 3, 2, "ua", "Різдво Христове" },
            { 4, 2, "en", "Christmas (Orthodox)" },
            // ...
        }
    );
}
```

## Що тестувати

- [ ] Міграції виконуються успішно
- [ ] Всі пресети створені в БД з правильними кодами
- [ ] Переклади UA/EN присутні для всіх пресетів та свят
- [ ] Рухомі свята мають правильний CalculationType
- [ ] Easter calculator розраховує правильні дати для 2026 року:
  - Православний Великдень 2026: 19 квітня
  - Католицький Великдень 2026: 5 квітня
- [ ] Можливість отримати свята пресету через API
- [ ] Фільтрація пресетів за типом працює
- [ ] Локалізовані назви відображаються залежно від мови

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-16
