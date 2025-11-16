# Task 27: E2E тестування user flows

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 6-8 годин
**Відповідальний AI**: Claude
**Залежить від**: Всі попередні

## Опис задачі

Написати E2E тести для критичних user flows використовуючи Playwright: реєстрація → створення календаря → додавання в кошик → checkout → оплата.

## Проблема

Потрібно автоматизоване тестування критичних шляхів користувача для запобігання regression bugs.

## Що треба зробити

1. **Налаштувати Playwright**
   ```bash
   npm install -D @playwright/test
   npx playwright install
   ```
   - Конфігурація browsers (Chromium, Firefox, WebKit)
   - Base URL
   - Screenshots on failure

2. **Написати тести для Authentication Flow**
   - Test: User Registration
     - Заповнити форму реєстрації
     - Verify email
     - Redirect до home
   - Test: User Login
     - Логін з credentials
     - Verify JWT token
     - Redirect до profile

3. **Написати тести для Calendar Creation Flow**
   - Test: Browse Catalog
     - Відкрити каталог
     - Відобразити шаблони
     - Фільтри працюють
   - Test: Create Calendar
     - Вибрати шаблон
     - Відкрити editor
     - Додати фото (mock upload)
     - Додати текст
     - Зберегти

4. **Написати тести для Checkout Flow**
   - Test: Add to Cart
     - Відкрити preview
     - Вибрати формат
     - Додати в кошик
     - Verify cart count
   - Test: Checkout Process
     - Заповнити контактні дані
     - Вибрати доставку
     - Verify total price
     - Create order

5. **Написати тести для Payment Flow (Mock)**
   - Test: Payment Success
     - Mock MonoBank redirect
     - Verify order status → Paid
     - Email notification sent

6. **Написати тести для Profile/Orders**
   - Test: View Orders
     - Navigate to profile/orders
     - Display order history
     - View order details
     - Download PDF (mock)

7. **Setup CI Integration**
   - Run tests on PR
   - Parallel execution
   - Test artifacts (screenshots, videos)

## Файли для створення/модифікації

- `playwright.config.ts`
- `tests/e2e/auth.spec.ts`
- `tests/e2e/catalog.spec.ts`
- `tests/e2e/editor.spec.ts`
- `tests/e2e/cart.spec.ts`
- `tests/e2e/checkout.spec.ts`
- `tests/e2e/payment.spec.ts`
- `tests/e2e/profile.spec.ts`
- `.github/workflows/e2e-tests.yml`

## Критерії успіху

- [ ] Всі критичні flows покриті тестами
- [ ] Тести проходять локально
- [ ] Тести проходять в CI
- [ ] Screenshots при помилках
- [ ] Parallel execution працює
- [ ] Test reports генеруються

## Залежності

- Всі попередні задачі повинні бути завершені

## Технічні деталі

### playwright.config.ts
```typescript
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',

  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],

  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
  },
})
```

### tests/e2e/auth.spec.ts
```typescript
import { test, expect } from '@playwright/test'

test.describe('Authentication', () => {
  test('user can register', async ({ page }) => {
    await page.goto('/auth/register')

    await page.fill('input[name="firstName"]', 'Іван')
    await page.fill('input[name="lastName"]', 'Петренко')
    await page.fill('input[name="email"]', `test${Date.now()}@example.com`)
    await page.fill('input[name="password"]', 'Password123!')
    await page.fill('input[name="confirmPassword"]', 'Password123!')

    await page.click('button[type="submit"]')

    await expect(page).toHaveURL('/')
    await expect(page.locator('text=Іван Петренко')).toBeVisible()
  })

  test('user can login', async ({ page }) => {
    await page.goto('/auth/login')

    await page.fill('input[name="email"]', 'test@example.com')
    await page.fill('input[name="password"]', 'Password123!')

    await page.click('button[type="submit"]')

    await expect(page).toHaveURL('/')
    await expect(page.locator('[data-testid="user-menu"]')).toBeVisible()
  })
})
```

### tests/e2e/checkout.spec.ts
```typescript
import { test, expect } from '@playwright/test'

test.describe('Checkout Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('/auth/login')
    await page.fill('input[name="email"]', 'test@example.com')
    await page.fill('input[name="password"]', 'Password123!')
    await page.click('button[type="submit"]')
    await page.waitForURL('/')
  })

  test('complete checkout flow', async ({ page }) => {
    // Add item to cart
    await page.goto('/catalog')
    await page.click('[data-testid="template-card"]:first-child')
    await page.click('button:has-text("Вибрати")')

    // Wait for editor
    await expect(page).toHaveURL(/\/editor/)

    // Add to cart from preview
    await page.click('[data-testid="preview-button"]')
    await page.selectOption('[data-testid="format-select"]', 'A4')
    await page.click('button:has-text("Додати в кошик")')

    // Go to cart
    await expect(page).toHaveURL('/cart')
    await expect(page.locator('[data-testid="cart-item"]')).toBeVisible()

    // Proceed to checkout
    await page.click('button:has-text("Оформити замовлення")')
    await expect(page).toHaveURL('/checkout')

    // Fill contact form
    await page.fill('input[name="firstName"]', 'Іван')
    await page.fill('input[name="lastName"]', 'Петренко')
    await page.fill('input[name="email"]', 'ivan@example.com')
    await page.fill('input[name="phone"]', '+380501234567')

    // Select delivery
    await page.click('input[value="nova-poshta-warehouse"]')
    await page.fill('input[placeholder="Почніть вводити назву міста"]', 'Київ')
    await page.click('text=Київ') // Select from dropdown

    // Submit order
    await page.click('button:has-text("Перейти до оплати")')

    // Verify redirect to payment
    await expect(page).toHaveURL(/\/payment/)
  })
})
```

### .github/workflows/e2e-tests.yml
```yaml
name: E2E Tests

on:
  pull_request:
    branches: [main, develop]

jobs:
  test:
    timeout-minutes: 60
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci

      - name: Install Playwright Browsers
        run: npx playwright install --with-deps

      - name: Run Playwright tests
        run: npx playwright test

      - uses: actions/upload-artifact@v3
        if: always()
        with:
          name: playwright-report
          path: playwright-report/
          retention-days: 30
```

## Примітки

- Playwright підтримує всі браузери
- Screenshots та videos при помилках
- Parallel execution прискорює тести
- CI integration важлива для regression testing

## Чому Claude?

Складна тестова задача:
- E2E test strategy
- Complex user flows
- Async operations handling
- CI/CD integration
- Test debugging та maintenance

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
