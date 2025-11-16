# Task 08: CI/CD для React frontend

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude
**Паралельно з**: Task 01, 02, 03, 04, 05, 06, 07

## Опис задачі

Налаштувати GitHub Actions для автоматичного build, test, та deploy Next.js frontend Customer Portal.

## Проблема

Потрібна автоматизація CI/CD для швидкого та надійного деплою фронтенду.

## Що треба зробити

1. **Створити GitHub Actions workflow**
   - `.github/workflows/frontend-ci.yml`
   - Trigger на push до main та PRs
   - Jobs: lint, type-check, test, build

2. **Налаштувати Lint job**
   - Run ESLint
   - Run Prettier check
   - Fail on errors

3. **Налаштувати Type-check job**
   - Run TypeScript compiler
   - Check for type errors

4. **Налаштувати Build job**
   - Run `npm run build`
   - Cache node_modules
   - Verify build success

5. **Налаштувати Deploy job**
   - Deploy to Vercel (recommended for Next.js)
   - Або до AWS S3 + CloudFront
   - Або до Azure Static Web Apps
   - Тільки для main branch

6. **Додати build optimization**
   - Environment variables для production
   - Minification
   - Tree shaking
   - Code splitting

## Файли для створення/модифікації

- `.github/workflows/frontend-ci.yml`
- `.github/workflows/frontend-cd.yml`
- `vercel.json` (якщо використовується Vercel)

## Критерії успіху

- [ ] CI workflow запускається на кожен PR
- [ ] Lint та type-check проходять автоматично
- [ ] Build job створює production bundle
- [ ] Deploy відбувається автоматично на main
- [ ] Failed builds блокують merge
- [ ] Deployment preview для PR (якщо Vercel)

## Залежності

- Task 01: Next.js проект повинен існувати

## Блокується наступні задачі

Немає (незалежна від інших задач)

## Технічні деталі

### .github/workflows/frontend-ci.yml
```yaml
name: Frontend CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]
    paths:
      - 'calendary-customer-portal/**'

jobs:
  lint:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./calendary-customer-portal

    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: './calendary-customer-portal/package-lock.json'

      - name: Install dependencies
        run: npm ci

      - name: Run ESLint
        run: npm run lint

      - name: Check formatting
        run: npx prettier --check .

  type-check:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./calendary-customer-portal

    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: './calendary-customer-portal/package-lock.json'

      - name: Install dependencies
        run: npm ci

      - name: Type check
        run: npx tsc --noEmit

  build:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./calendary-customer-portal

    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: './calendary-customer-portal/package-lock.json'

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build
        env:
          NEXT_PUBLIC_API_URL: ${{ secrets.NEXT_PUBLIC_API_URL }}

      - name: Upload build artifacts
        uses: actions/upload-artifact@v3
        with:
          name: build
          path: ./calendary-customer-portal/.next
```

### .github/workflows/frontend-cd.yml
```yaml
name: Frontend CD

on:
  push:
    branches: [main]
    paths:
      - 'calendary-customer-portal/**'

jobs:
  deploy:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./calendary-customer-portal

    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install Vercel CLI
        run: npm install --global vercel@latest

      - name: Pull Vercel Environment
        run: vercel pull --yes --environment=production --token=${{ secrets.VERCEL_TOKEN }}

      - name: Build Project
        run: vercel build --prod --token=${{ secrets.VERCEL_TOKEN }}

      - name: Deploy to Vercel
        run: vercel deploy --prebuilt --prod --token=${{ secrets.VERCEL_TOKEN }}
```

### vercel.json
```json
{
  "buildCommand": "npm run build",
  "devCommand": "npm run dev",
  "installCommand": "npm install",
  "framework": "nextjs",
  "outputDirectory": ".next"
}
```

## Примітки

- Vercel - найкраща платформа для Next.js (створена тими ж авторами)
- GitHub Actions безкоштовні для публічних репозиторіїв
- Caching node_modules значно прискорює builds
- Environment variables повинні бути в GitHub Secrets

## Чому Claude?

DevOps задача:
- Налаштування CI/CD pipeline
- GitHub Actions configuration
- Deployment strategy
- Потрібне розуміння automation

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
