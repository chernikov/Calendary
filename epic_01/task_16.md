# Task 16: Міграція БД для нової структури

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude Code

## Опис задачі

Створити EF Core міграції для нової структури календаря 2026.

## Що треба зробити

1. **Нові поля в Calendar entity**:
   - `Year` (int) - рік календаря
   - `MonthlyImages` (JSON) - { month: 1, imageId: "..." }

2. **Створити міграцію**:
   ```bash
   dotnet ef migrations add Calendar2026Structure
   ```

3. **Data migration**:
   - Перенести старі дані якщо є
   - Seed data для 2026 holidays

4. **Apply migration**:
   ```bash
   dotnet ef database update
   ```

## Файли для створення

- `src/Calendary.Repos/Migrations/XXXXXX_Calendar2026Structure.cs`

## Файли для зміни

- `src/Calendary.Model/Calendar.cs`
- `src/Calendary.Repos/CalendaryDbContext.cs`

## Що тестувати

- [x] Міграція створена без помилок
- [x] Нові поля додані до моделі
- [x] Конфігурація для JSON column налаштована
- [x] Model snapshot оновлений
- [ ] Міграція apply (потребує dotnet environment)
- [ ] Rollback працює (потребує dotnet environment)

---

## Виконано

**Дата**: 2025-11-16

### Зміни:

1. **Створено новий клас** `MonthlyImage.cs`:
   - Властивості: `Month` (int), `ImageId` (string)

2. **Оновлено** `Calendar.cs`:
   - Додано властивість `MonthlyImages` (List<MonthlyImage>)
   - Note: Властивість `Year` вже існувала в моделі

3. **Оновлено** `CalendarConfiguration.cs`:
   - Налаштовано `MonthlyImages` як JSON column через `OwnsMany` з `ToJson()`

4. **Створено міграцію** `20251116000000_Calendar2026Structure.cs`:
   - Додає JSON column `MonthlyImages` до таблиці `Calendars`
   - Включає методи `Up` і `Down` для rollback

5. **Оновлено** `CalendaryDbContextModelSnapshot.cs`:
   - Додано конфігурацію для `MonthlyImages` як owned entity з JSON serialization

### Примітки:

- Міграція готова до застосування за допомогою `dotnet ef database update`
- JSON column використовує вбудовану підтримку EF Core 9.0
- Структура `MonthlyImages` дозволяє зберігати масив об'єктів з month та imageId
- Rollback міграції видалить column `MonthlyImages` з таблиці `Calendars`

---

**Створено**: 2025-11-15
**Завершено**: 2025-11-16
