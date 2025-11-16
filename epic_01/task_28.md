# Task 28: Preview PDF перед завантаженням

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P2 (Середній)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: GPT/Codex

## Опис задачі

Дати можливість preview PDF перед завантаженням.

## Що треба зробити

1. **PDF Viewer**:
   - Використати `ng2-pdf-viewer` або `pdfjs`
   - Показати всі сторінки
   - Zoom in/out
   - Navigation (next/prev page)

2. **Generate Preview**:
   - Backend endpoint `/api/calendar/preview/{calendarId}` (lower quality)
   - Швидша генерація (quality 60%)
   - Водяний знак "PREVIEW" по центру кожної сторінки
   - Opacity: 30%
   - Кут: 45°
   - Показується ТІЛЬКИ на preview PDF

3. **Actions**:
   - Download (full quality, БЕЗ будь-яких водяних знаків)
   - Edit (повернутись до editor)
   - Share (future feature)

**Важливо**: 
- Preview PDF має водяний знак "PREVIEW" - для швидкого перегляду
- Фінальний PDF (download) БЕЗ водяних знаків - чиста версія для друку

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/pdf-preview/pdf-preview.component.ts`

## Файли для зміни

- `src/Calendary.Api/Controllers/CalendarController.cs` - preview endpoint

## Що тестувати

- [ ] Preview відображається
- [ ] Navigation працює
- [ ] Zoom працює
- [ ] Download повної версії працює

---

**Створено**: 2025-11-15
