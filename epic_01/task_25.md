# Task 25: Новий PDF генератор для календаря 2026

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 6-8 годин
**Відповідальний AI**: GPT/Codex

## Опис задачі

Створити новий сервіс для генерації PDF календарів високої якості.

## Що треба зробити

1. **PdfCalendarGenerator**:
   - Використати QuestPDF або iTextSharp
   - Формат A3 (297 x 420 мм)
   - 300 DPI мінімум
   - Кожен місяць - окрема сторінка

2. **Page Layout**:
   ```
   +---------------------------+
   | Image (top 60% of page)   |
   |                           |
   |                           |
   +---------------------------+
   | СІЧЕНЬ 2026               |
   | Пн Вт Ср Чт Пт Сб Нд      |
   | Calendar grid             |
   | Holidays highlighted      |
   +---------------------------+
   ```

3. **Features**:
   - High quality image rendering
   - Custom fonts (Ukrainian support)
   - Color schemes from preset
   - Watermark (optional)
   - Metadata (author, creation date)

4. **Optimization**:
   - Compress images (lossy, quality 85%)
   - Limit PDF size <15MB
   - Background generation (queue)

## Файли для створення

- `src/Calendary.Core/Services/PdfCalendarGenerator.cs`
- `src/Calendary.Core/Models/PdfGenerationOptions.cs`

## Файли для зміни

- `src/Calendary.Api/Controllers/CalendarController.cs`

## Що тестувати

- [ ] PDF генерується успішно
- [ ] Всі 12 місяців в PDF
- [ ] Зображення високої якості
- [ ] Holidays виділені
- [ ] Розмір <15MB
- [ ] Генерація <10 секунд

---

**Створено**: 2025-11-15
