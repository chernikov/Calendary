# Task 27: Водяний знак та метадані PDF

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P2 (Середній)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: GPT/Codex

## Опис задачі

Додати водяний знак "Calendary.com.ua" та метадані в PDF.

## Що треба зробити

1. **Watermark**:
   - Прозорий текст "Calendary.com.ua"
   - Розташування: bottom right
   - Opacity: 10%
   - Font size: 10pt

2. **PDF Metadata**:
   - Title: "Календар 2026"
   - Author: User name або "Calendary"
   - Subject: "Персональний календар"
   - Keywords: "календар, 2026, україна"
   - Creator: "Calendary.com.ua"
   - Creation Date

3. **Optional**: QR Code
   - Link to Calendary.com.ua
   - Small size в footer

## Файли для зміни

- `src/Calendary.Core/Services/PdfCalendarGenerator.cs`

## Що тестувати

- [ ] Watermark видимий але ненав'язливий
- [ ] Metadata коректна
- [ ] QR code працює (якщо додано)

---

**Створено**: 2025-11-15
