# Task 17: Новий сервіс формування календаря

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 4-6 годин
**Відповідальний AI**: Gemini

## Опис задачі

Створити новий CalendarBuilderService для генерації календаря 2026.

## Що треба зробити

1. **CalendarBuilderService**:
   ```csharp
   public class CalendarBuilderService
   {
       public async Task<Calendar> BuildCalendar2026Async(
           Dictionary<int, string> monthlyImages,
           string language,
           DayOfWeek startDay
       )
       {
           // Generate calendar grid
           // Apply holidays
           // Assign images
       }
   }
   ```

2. **Методи**:
   - `GenerateMonthGrid(int year, int month)` - сітка днів
   - `ApplyHolidays(int year, string country)` - додати свята
   - `AssignImages(monthlyImages)` - призначити зображення
   - `Validate()` - перевірка коректності

3. **Підтримка локалізації**:
   - Назви місяців (UA, EN)
   - Назви днів тижня
   - Формати дат

## Файли для створення

- `src/Calendary.Core/Services/CalendarBuilderService.cs`
- `src/Calendary.Core/Models/CalendarGrid.cs`

## Що тестувати

- [ ] Календар генерується для 2026
- [ ] Всі 12 місяців коректні
- [ ] Holidays застосовуються
- [ ] Зображення призначаються правильно
- [ ] Локалізація працює (UA/EN)

---

**Створено**: 2025-11-15
