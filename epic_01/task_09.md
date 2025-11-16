# Task 09: Можливість називати моделі

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Claude

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

- [x] Назва зберігається в БД
- [x] Валідація працює (min 3, max 50 chars)
- [x] UI показує назву замість ID
- [x] Можна перейменувати модель

---

## Виконано

**Дата завершення**: 2025-11-16

### Реалізований функціонал:

**Backend:**
- ✅ FluxModel.Name поле вже існувало в src/Calendary.Model/FluxModel.cs:7
- ✅ API endpoint PUT /api/flux-model/{id}/name для оновлення назви (FluxModelController.cs:175-214)
- ✅ Валідація довжини назви (3-50 символів) в контролері
- ✅ Перевірка унікальності назви для користувача (FluxModelRepository.IsNameUniqueForUserAsync)
- ✅ FluxModelService.ChangeNameAsync для обробки логіки перейменування
- ✅ UpdateNameRequest DTO
- ✅ FluxModelDto містить поле Name
- ✅ FluxModelConfiguration оновлено з обмеженням MaxLength(50)

**Frontend:**
- ✅ FluxModelService.updateName метод для виклику API
- ✅ ModelsPanelComponent з повним UI для редагування назви моделі
- ✅ Inline редагування з кнопками збереження/скасування
- ✅ Відображення назви моделі в списку моделей
- ✅ Меню дій з опцією "Перейменувати"

**База даних:**
- ✅ Поле Name існує в міграції InitialCreate
- ✅ Конфігурація FluxModelConfiguration з обмеженням MaxLength(50)

### Зміни в коді:
- `src/Calendary.Repos/Configurations/FluxModelConfiguration.cs` - додано конфігурацію валідації Name

---

**Створено**: 2025-11-15
**Завершено**: 2025-11-16
