# Task 20: UI для попереднього перегляду календаря

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Claude Code
**Виконано**: 2025-11-16

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

- [x] Preview показує всі 12 місяців
- [x] Зображення відображаються
- [x] Holidays виділяються
- [x] Navigation працює
- [x] Customization змінює вигляд
- [x] Responsive design

## Реалізація

### Створені компоненти:

1. **MonthPageComponent** (`src/Calendary.Ng/src/app/pages/editor/components/month-page/`)
   - Відображення окремого місяця з зображенням
   - Календарна сітка з днями місяця
   - Виділення вихідних та свят
   - Підтримка customization (шрифти, кольори, розташування)

2. **CalendarGridModel** (`src/Calendary.Ng/src/app/pages/editor/models/calendar-grid.model.ts`)
   - Моделі даних: CalendarDay, CalendarMonth, CalendarCustomization
   - Функція генерації календарної сітки з підтримкою свят
   - Налаштування за замовчуванням

3. **Розширений CalendarPreviewComponent**
   - Два режими перегляду: grid (сітка місяців) та preview (попередній перегляд)
   - Навігація між місяцями (prev/next, селектор)
   - Zoom controls (50-200%, з кроком 10%)
   - Fullscreen режим
   - Edit mode для зміни зображень
   - Generate PDF button
   - Customization panel:
     - Вибір шрифту (6 опцій)
     - Розмір шрифту (10-24px)
     - Кольори (основний, свята, вихідні)
     - Розташування зображення (зверху/знизу)
   - Збереження налаштувань в localStorage

### Особливості:
- Responsive design з підтримкою mobile
- Print-friendly стилі
- Інтеграція зі святами з backend
- Збереження стану та налаштувань
- Fullscreen API integration

---

**Створено**: 2025-11-15
**Виконано**: 2025-11-16
