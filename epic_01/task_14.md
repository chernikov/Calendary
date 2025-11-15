# Task 14: Збереження історії промптів

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P2 (Середній)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Full Stack / Claude Code

## Опис задачі

Зберігати історію використаних промптів для швидкого повторного використання.

## Що треба зробити

1. **LocalStorage для історії**:
   - Зберігати останні 20 промптів
   - Dropdown з історією
   - Click to reuse

2. **Backend (optional)**:
   - Зберігати в БД (таблиця PromptHistory)
   - Синхронізація між пристроями

3. **UI**:
   - History dropdown біля textarea
   - Search в історії
   - Pin favorite prompts

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/services/prompt-history.service.ts`

## Що тестувати

- [ ] Історія зберігається
- [ ] Dropdown показує останні промпти
- [ ] Click to reuse працює
- [ ] Max 20 items
- [ ] Favorites працюють (якщо реалізовано)

---

**Створено**: 2025-11-15
