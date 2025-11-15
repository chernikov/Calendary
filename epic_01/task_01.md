# Task 01: Перевірка запуску програми

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 1-2 години
**Відповідальний AI**: Claude

## Опис задачі

Перевірити що вся програма коректно запускається локально та на dev середовищі. Це базова задача перед початком розробки нових фіч.

## Проблема

Перед додаванням нових функцій необхідно переконатися що існуюча система працює стабільно.

## Що треба зробити

1. ✅ Перевірити запуск Backend:
   - `dotnet restore`
   - `dotnet build`
   - `dotnet run --project src/Calendary.Api`
   - Перевірити що API доступний на http://localhost:5000

2. ✅ Перевірити запуск Frontend:
   - `cd src/Calendary.Ng`
   - `npm install`
   - `npm start`
   - Перевірити що Angular запустився на http://localhost:4200

3. ✅ Перевірити БД:
   - PostgreSQL/SQL Server запущений
   - Connection string коректний
   - Міграції застосовані: `dotnet ef database update`

4. ✅ Перевірити Consumer (RabbitMQ):
   - `dotnet run --project src/Calendary.Consumer`
   - RabbitMQ доступний

5. ✅ Перевірити Docker Compose:
   - `docker-compose up`
   - Всі сервіси запускаються

## Файли для перевірки

- `src/Calendary.Api/Program.cs`
- `src/Calendary.Ng/package.json`
- `docker-compose.yml`
- `appsettings.json` / `appsettings.Development.json`

## Що тестувати

- [ ] Backend запускається без помилок
- [ ] Frontend запускається без помилок
- [ ] БД підключення працює
- [ ] Swagger доступний на /swagger
- [ ] Головна сторінка Angular відображається
- [ ] RabbitMQ Consumer підключається
- [ ] Docker Compose піднімає всі контейнери
- [ ] Логи не містять критичних помилок

## Критерії успіху

- ✅ `dotnet run` запускає API без помилок
- ✅ `npm start` запускає Angular без помилок
- ✅ Swagger UI доступний та показує endpoints
- ✅ Головна сторінка завантажується в браузері
- ✅ Немає помилок в консолі браузера

## Залежності

Немає (це перша задача)

## Блокери

- Відсутні налаштування БД
- Неправильні credentials для Replicate API
- Проблеми з RabbitMQ

## Примітки

Це базова задача яка має бути виконана ПЕРШОЮ перед усіма іншими.

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
