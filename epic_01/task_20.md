# Task 20: UI для попереднього перегляду календаря

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: GPT/Codex

## Опис задачі

Створити UI для preview календаря перед генерацією PDF.

## Що треба зробити

1. **Calendar Preview Component**:
   - Month-by-month view
   - Зображення для кожного місяця
   - Сітка днів з holidays
   - Navigation (prev/next month)

2. **Month Page Layout**:
   ```
   +------------------------+
   | [Image for month]      |
   |                        |
   +------------------------+
   | Січень 2026            |
   | Пн Вт Ср Чт Пт Сб Нд   |
   |       1  2  3  4  5    |
   | 6  7  8  9 10 11 12    |
   | ...                    |
   +------------------------+
   ```

3. **Controls**:
   - Zoom in/out
   - Full screen preview
   - Edit mode (change image for month)
   - Generate PDF button

4. **Customization**:
   - Font selection
   - Color scheme
   - Layout (image top/bottom)

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/calendar-preview/calendar-preview.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/month-page/month-page.component.ts`

## Що тестувати

- [ ] Preview показує всі 12 місяців
- [ ] Зображення відображаються
- [ ] Holidays виділяються
- [ ] Navigation працює
- [ ] Customization змінює вигляд
- [ ] Responsive design

---

**Створено**: 2025-11-15
