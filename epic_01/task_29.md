# Task 29: E2E тести для /editor

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P2 (Середній)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: QA / Claude Code

## Опис задачі

Створити автоматизовані E2E тести для основних сценаріїв в /editor.

## Що треба зробити

1. **Setup Playwright або Cypress**:
   ```bash
   npm install --save-dev @playwright/test
   ```

2. **Test Scenarios**:
   - Login → Navigate to /editor
   - Select active model
   - Generate image
   - Add image to calendar month
   - Preview calendar
   - Generate PDF

3. **Test Cases**:
   ```typescript
   test('should generate image and add to calendar', async ({ page }) => {
     await page.goto('/editor');
     await page.click('[data-test="generate-button"]');
     await page.fill('[data-test="prompt"]', 'Test prompt');
     await page.click('[data-test="generate-submit"]');
     await page.waitForSelector('[data-test="generated-image"]');
     await page.click('[data-test="add-to-calendar"]');
     await page.selectOption('[data-test="month-selector"]', '1');
     expect(await page.locator('[data-test="month-1-image"]')).toBeVisible();
   });
   ```

4. **Visual Regression Tests**:
   - Screenshot comparison
   - PDF comparison

## Файли для створення

- `tests/e2e/editor.spec.ts`
- `tests/e2e/calendar-generation.spec.ts`
- `playwright.config.ts`

## Що тестувати

- [ ] Всі критичні user flows працюють
- [ ] Тести проходять на CI/CD
- [ ] Screenshots співпадають
- [ ] Cross-browser testing (Chrome, Firefox, Safari)

---

**Створено**: 2025-11-15
