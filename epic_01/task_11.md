# Task 11: Вибір активної моделі

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Gemini

## Опис задачі

Дозволити користувачу вибрати яка модель використовується для генерації зображень.

## Що треба зробити

1. **Backend**: Endpoint `POST /api/flux-models/{id}/set-active`
2. **Frontend**: Click на модель → set active
3. **State**: Зберігати active model в service/state
4. **Generation**: Використовувати active model при генерації

## Файли для зміни

- `src/Calendary.Api/Controllers/FluxModelController.cs`
- `src/Calendary.Ng/src/app/pages/editor/services/flux-model.service.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/generate-modal/`

## Що тестувати

- [ ] Set active працює
- [ ] UI показує активну модель
- [ ] Генерація використовує активну модель
- [ ] Після reload - active model зберігається

---

**Створено**: 2025-11-15
