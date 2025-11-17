# Backlog: Infrastructure & DevOps

## P2-06: Структуроване логування з Serilog

**Джерело**: TASKS.md (Task 6)
**Пріоритет**: P2 (Середній)
**Складність**: Низька
**Час**: 2-3 години

### Що треба зробити
1. Serilog NuGet packages
2. Sinks: Console, File, Seq
3. Логування API requests/responses (middleware)
4. Логування Replicate API calls
5. Structured logging

### Файли
- `src/Calendary.Api/Program.cs`
- `src/Calendary.Api/Calendary.Api.csproj`

### Приклад
```csharp
Log.Information("User {UserId} created order {OrderId}", userId, orderId)
```

---

## INFRA-2: Docker production setup

**Джерело**: TASKS.md (Infra 2)
**Пріоритет**: P2 (Середній)
**Складність**: Середня
**Час**: 2-3 години

### Що треба зробити
1. Оптимізувати Dockerfile (multi-stage build)
2. Health check endpoints
3. docker-compose для production
4. Reverse proxy (nginx)

### Файли
- `Dockerfile.production`
- `docker-compose.production.yml`
- `nginx.conf`

---

## P3-09: Міграція на .NET 10

**Джерело**: TASKS.md (Task 9)
**Пріоритет**: P3 (Низький)
**Складність**: Низька
**Час**: 1-2 години
**Статус**: Чекає релізу .NET 10

### Що треба зробити
1. Оновити `.csproj`: `net10.0`
2. Оновити NuGet packages
3. Запустити тести
4. Перевірити breaking changes

---

**Створено**: 2025-11-15
