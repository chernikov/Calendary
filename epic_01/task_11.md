# Task 11: Вибір активної моделі

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
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

- [x] Set active працює
- [x] UI показує активну модель
- [x] Генерація використовує активну модель
- [x] Після reload - active model зберігається

## Виконано

- ✅ Backend endpoint `POST /api/flux-models/{id}/set-active` вже був реалізований
- ✅ Додано візуальний індикатор (зірка) для активної моделі в UI
- ✅ Додано опцію "Зробити активною" в меню моделі
- ✅ Оновлено `GetCurrentByUserIdAsync` для повернення активної моделі
- ✅ Генерація використовує активну модель через `activeModel.id`
- ✅ Активна модель зберігається в базі даних (поле `IsActive`)

---

**Створено**: 2025-11-15
**Завершено**: 2025-11-16
