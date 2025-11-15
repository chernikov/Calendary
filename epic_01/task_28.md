# Task 28: Preview PDF перед завантаженням

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P2 (Середній)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Frontend Dev / Claude Code

## Опис задачі

Дати можливість preview PDF перед завантаженням.

## Що треба зробити

1. **PDF Viewer**:
   - Використати `ng2-pdf-viewer` або `pdfjs`
   - Показати всі сторінки
   - Zoom in/out
   - Navigation (next/prev page)

2. **Generate Preview**:
   - Backend endpoint `/api/calendar/preview` (lower quality)
   - Швидша генерація (quality 60%)
   - Watermark "PREVIEW"

3. **Actions**:
   - Download (full quality)
   - Edit (повернутись до editor)
   - Share (future feature)

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
