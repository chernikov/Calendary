# Backlog: Performance & Optimization

## P1-01: Оптимізація генерації PDF календарів

**Джерело**: TASKS.md (Task 1)
**Пріоритет**: P1 (Високий)
**Складність**: Висока
**Час**: 4-6 годин

### Проблема
Генерація PDF для календарів займає 10-15 секунд. Потрібна оптимізація.

### Що треба зробити
1. Паралельна обробка зображень
2. Використовувати кешовані thumbnail версії
3. Фонова генерація через RabbitMQ
4. Progress bar (WebSocket/SignalR)

### Файли
- `src/Calendary.Core/Services/PdfService.cs`
- `src/Calendary.Api/Controllers/CalendarController.cs`
- `src/Calendary.Consumer/` (новий Consumer)

---

## P1-04: Кешування результатів Replicate API

**Джерело**: TASKS.md (Task 4)
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 2-3 години

### Проблема
Ідентичні промпти генерують знову замість використання кешу.

### Що треба зробити
1. Redis або in-memory cache
2. Ключ: `{modelVersion}:{prompt}:{seed}`
3. TTL: 30 днів
4. БД таблиця `GeneratedImages`

### Файли
- `src/Calendary.Core/Services/ReplicateService.cs`
- `src/Calendary.Repos/Repositories/GeneratedImageRepository.cs`
- `docker-compose.yml` (додати Redis)

---

## P2-08: Оптимізація запитів до БД (N+1 problem)

**Джерело**: TASKS.md (Task 8)
**Пріоритет**: P2 (Середній)
**Складність**: Середня
**Час**: 3-4 години

### Проблема
N+1 queries problem в репозиторіях.

### Що треба зробити
1. SQL logging (dev only)
2. `.Include()` та `.ThenInclude()`
3. `.AsNoTracking()` для read-only
4. Compiled queries

### Файли
- `src/Calendary.Repos/Repositories/*.cs`
- `src/Calendary.Core/Services/*.cs`

---

**Створено**: 2025-11-15
