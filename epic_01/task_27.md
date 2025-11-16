# Task 27: Водяний знак та метадані PDF

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P2 (Середній)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: GPT/Codex

## Опис задачі

Додати метадані в PDF. Водяний знак "PREVIEW" показується тільки на preview версії (Task 28).

## Що треба зробити

1. **PDF Metadata**:
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

- [ ] Metadata коректна
- [ ] QR code працює (якщо додано)
- [ ] Фінальний PDF без водяних знаків

---

**Створено**: 2025-11-15
