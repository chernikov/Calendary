# Task 15: Видалення старого коду календаря

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Claude

## Опис задачі

Видалити застарілий код формування календаря який більше не використовується.

## Що треба зробити

1. **Знайти старий код**:
   - `CalendarService.cs` - старі методи
   - `PdfGeneratorService.cs` - deprecated методи
   - Старі контролери/endpoints

2. **Backup перед видаленням**:
   - Створити git branch для backup
   - Зберегти в окремій папці `/legacy/`

3. **Видалити**:
   - Закоментувати спочатку
   - Тестувати що нічого не зламалось
   - Потім видалити повністю

4. **Update документацію**:
   - README.md
   - API docs

## Файли для перевірки

- `src/Calendary.Core/Services/CalendarService.cs`
- `src/Calendary.Core/Services/PdfGeneratorService.cs`
- `src/Calendary.Api/Controllers/CalendarController.cs`

## Що тестувати

- [ ] Програма запускається після видалення
- [ ] Всі тести проходять
- [ ] API endpoints працюють
- [ ] Немає references на видалений код

---

**Створено**: 2025-11-15
