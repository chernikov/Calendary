# Task 21: Створення системи пресетів

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude

## Опис задачі

Створити систему пресетів для швидкого створення тематичних календарів.

## Що треба зробити

1. **CalendarPreset entity**:
   - Name (UA, EN)
   - Description
   - Theme (новорічний, різдвяний, весняний...)
   - Prompts для кожного місяця
   - Color scheme
   - IsPublic (доступний всім чи тільки автору)

2. **Backend API**:
   - `GET /api/presets` - список пресетів
   - `GET /api/presets/{id}` - details
   - `POST /api/presets/{id}/apply` - застосувати до календаря

3. **Apply Preset logic**:
   - Згенерувати 12 зображень на основі промптів пресета
   - Застосувати color scheme
   - Призначити до місяців автоматично

## Файли для створення

- `src/Calendary.Model/CalendarPreset.cs`
- `src/Calendary.Core/Services/PresetService.cs`
- `src/Calendary.Api/Controllers/PresetController.cs`

## Що тестувати

- [ ] Пресети зберігаються в БД
- [ ] API endpoints працюють
- [ ] Apply preset генерує 12 зображень
- [ ] Color scheme застосовується

---

**Створено**: 2025-11-15
