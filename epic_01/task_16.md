# Task 16: Міграція БД для нової структури

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Gemini

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

- [ ] Міграція apply without errors
- [ ] Нові поля створені в БД
- [ ] Старі дані збереглися (якщо є)
- [ ] Seed data завантажився
- [ ] Rollback працює

---

**Створено**: 2025-11-15
