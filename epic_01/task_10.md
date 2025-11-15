# Task 10: Список моделей користувача

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Frontend Dev / Claude Code

## Опис задачі

Створити UI для відображення всіх моделей користувача в /editor.

## Що треба зробити

1. **Models Panel** в sidebar:
   - Список всіх FluxModel
   - Назва, дата створення, статус
   - Active model highlighted
   - Click to select

2. **Model Card**:
   - Name (editable)
   - Status (training, ready, failed)
   - Created date
   - Actions: Select, Rename, Delete

3. **Create Model Button**:
   - Redirect на training page
   - Або inline form для upload photos

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/models-panel/models-panel.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/services/flux-model.service.ts`

## Що тестувати

- [ ] Список моделей завантажується
- [ ] Active model виділяється
- [ ] Click to select працює
- [ ] Create button працює
- [ ] Empty state показується якщо немає моделей

---

**Створено**: 2025-11-15
