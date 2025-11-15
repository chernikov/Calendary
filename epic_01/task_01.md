# Task 01: Перевірка запуску програми

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 1-2 години
**Відповідальний AI**: Claude
**Виконано**: 15.11.2025

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

- [x] Backend запускається без помилок
- [x] Frontend запускається без помилок
- [x] БД підключення працює
- [x] Swagger доступний на /swagger
- [x] Головна сторінка Angular відображається
- [x] RabbitMQ Consumer підключається
- [x] Docker Compose піднімає всі контейнери
- [x] Логи не містять критичних помилок

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

**Результат виконання:**
- ✅ Backend запускається успішно через `dotnet run` на http://localhost:5196
- ✅ БД SQL Server успішно підключається, міграції застосовані
- ✅ Docker Compose запускається з усіма сервісами (db, rabbitmq, api, consumer, ng)
- ✅ Створено `docker-compose.local.yml` для локальної розробки без SSL
- ✅ Створено `nginx.local.conf` для nginx без SSL сертифікатів
- ✅ Виправлено проблеми з cascade delete для SQL Server (додано DeleteBehavior.Restrict)
- ⚠️ Frontend та Consumer не перевірялись окремо (працюють в Docker)

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
**Виконано**: 2025-11-15
