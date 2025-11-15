# Task 09: Можливість називати моделі

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Full Stack / Claude Code

## Опис задачі

Додати можливість давати власні назви Flux моделям замість дефолтних.

## Що треба зробити

1. **Backend**: Додати поле `Name` в FluxModel entity
2. **API**: Endpoint для rename моделі
3. **Frontend**: UI для редагування назви
4. **Validation**: Назва 3-50 символів, унікальна для користувача

## Файли для зміни

- `src/Calendary.Model/FluxModel.cs` - додати поле Name
- `src/Calendary.Repos/Migrations/` - нова міграція
- `src/Calendary.Api/Controllers/FluxModelController.cs` - метод UpdateName
- `src/Calendary.Ng/src/app/pages/editor/` - UI для rename

## Що тестувати

- [ ] Назва зберігається в БД
- [ ] Валідація працює (min 3, max 50 chars)
- [ ] UI показує назву замість ID
- [ ] Можна перейменувати модель

---

**Створено**: 2025-11-15
