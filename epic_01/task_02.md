# Task 02: Налаштування CI/CD

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: DevOps AI / Claude Code

## Опис задачі

Налаштувати автоматичний CI/CD pipeline для автоматичного build, тестування та deploy.

## Проблема

Ручний deploy займає багато часу та схильний до помилок. Потрібна автоматизація.

## Що треба зробити

1. **GitHub Actions для Build**:
   - Створити `.github/workflows/build.yml`
   - Автоматичний build на кожен push
   - Запуск unit тестів
   - Перевірка code quality

2. **GitHub Actions для Tests**:
   - Запуск всіх тестів на кожен PR
   - Code coverage report
   - Блокування merge якщо тести падають

3. **Deploy на Staging**:
   - Автоматичний deploy після merge в `main`
   - Docker build and push
   - Deploy на staging сервер

4. **Deploy на Production**:
   - Тільки після тега (v1.0.0)
   - Manual approval required
   - Rollback mechanism

## Файли для створення

- `.github/workflows/build.yml`
- `.github/workflows/test.yml`
- `.github/workflows/deploy-staging.yml`
- `.github/workflows/deploy-production.yml`
- `Dockerfile.production` (оптимізований)

## Файли для зміни

- `docker-compose.yml` (додати production config)
- `README.md` (додати badges CI/CD)

## Що тестувати

- [ ] Build workflow запускається на push
- [ ] Tests workflow запускається на PR
- [ ] Failed tests блокують merge
- [ ] Staging deploy працює після merge
- [ ] Production deploy працює після тега
- [ ] Docker images створюються коректно
- [ ] Environment variables правильно передаються

## Критерії успіху

- ✅ CI/CD pipeline повністю автоматизований
- ✅ Тести запускаються автоматично
- ✅ Deploy на staging після merge
- ✅ Manual approval для production
- ✅ Badges в README показують статус

## Залежності

- [Task 01](./task_01.md) - Програма має запускатись

## Environment Variables

```env
# Staging
DATABASE_URL=postgresql://...
REPLICATE_API_KEY=...
RABBITMQ_URL=...

# Production
DATABASE_URL=postgresql://...
REPLICATE_API_KEY=...
RABBITMQ_URL=...
```

## Приклад workflow

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
