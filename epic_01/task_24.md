# Task 24: Застосування пресета до календаря

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude

## Опис задачі

Реалізувати логіку застосування пресета: генерація 12 зображень та призначення до місяців.

## Що треба зробити

1. **Backend endpoint**:
   ```csharp
   [HttpPost("presets/{id}/apply")]
   public async Task<IActionResult> ApplyPreset(string id)
   {
       var preset = await _presetService.GetByIdAsync(id);
       var userId = User.GetUserId();

       // Generate 12 images based on preset prompts
       var images = new List<string>();
       for (int month = 1; month <= 12; month++)
       {
           var prompt = preset.MonthlyPrompts[month];
           var imageUrl = await _replicateService.GenerateImageAsync(prompt);
           images.Add(imageUrl);
       }

       // Assign to calendar
       await _calendarBuilder.AssignImages(userId, images);

       return Ok(new { images });
   }
   ```

2. **Batch generation**:
   - Генерувати по 3-4 зображення паралельно
   - Progress updates через SignalR
   - Estimated time: 3-5 хвилин для 12 зображень

3. **Error handling**:
   - Якщо 1-2 зображення failed - retry
   - Якщо >3 failed - скасувати весь пресет
   - Rollback changes

## Файли для зміни

- `src/Calendary.Api/Controllers/PresetController.cs`
- `src/Calendary.Core/Services/PresetService.cs`
- `src/Calendary.Ng/src/app/pages/editor/services/preset.service.ts`

## Що тестувати

- [ ] 12 зображень генеруються
- [ ] Progress показується
- [ ] Зображення призначаються до місяців
- [ ] Color scheme застосовується
- [ ] Error handling працює
- [ ] Rollback при помилках

---

**Створено**: 2025-11-15
