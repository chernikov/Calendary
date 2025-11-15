# Task 02: Налаштування CI/CD

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude
**Дата завершення**: 2025-11-15

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

## Результати виконання

### Створені файли:

1. **GitHub Actions Workflows**:
   - `.github/workflows/build.yml` - Build та тести для всіх компонентів
   - `.github/workflows/test.yml` - PR тести з code coverage та quality checks
   - `.github/workflows/deploy-staging.yml` - Автоматичний deploy на staging
   - `.github/workflows/deploy-production.yml` - Production deploy з manual approval

2. **Оптимізовані Dockerfiles**:
   - `Dockerfile.production` - Оптимізований для API (Alpine, multi-stage)
   - `src/Calendary.Consumer/Dockerfile.production` - Оптимізований для Consumer
   - `src/Calendary.Ng/Dockerfile.production` - Оптимізований для Frontend

3. **Оновлено**:
   - `README.md` - Додано CI/CD badges

### Покращення:

- Використання GitHub Actions v4 (замість v2)
- Multi-stage Docker builds для мінімізації розміру образів
- Alpine images для меншого розміру
- Non-root користувачі в контейнерах (безпека)
- Health checks для всіх сервісів
- Code coverage reports
- Security scanning (Trivy)
- Gzip compression для frontend
- Кешування залежностей в workflows
- Manual approval для production deploy
- Rollback mechanism для production
- Automated backup перед production deploy

### Наступні кроки:

1. Налаштувати GitHub Environments:
   - `staging` environment з STAGING_URL та DIGITAL_OCEAN_HOST_IP
   - `production` environment з manual approval та PRODUCTION_URL, PRODUCTION_HOST_IP

2. Додати GitHub Secrets:
   - `DOCKER_PASSWORD` - Docker Hub password
   - `DIGITALOCEAN_SSHKEY` - SSH key для staging
   - `PRODUCTION_SSHKEY` - SSH key для production

3. Налаштувати branch protection rules для `main`

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
**Завершено**: 2025-11-15
