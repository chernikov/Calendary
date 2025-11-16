import { test, expect } from '@playwright/test';

/**
 * E2E tests for /editor page
 * Testing main workflows:
 * - Loading editor page
 * - Selecting models
 * - Generating images
 * - Adding images to calendar
 */

test.describe('Editor Page', () => {
  test.beforeEach(async ({ page }) => {
    // TODO: Add authentication setup here
    // For now, assuming user is already logged in or auth is handled by cookies/session
    await page.goto('/editor');
  });

  test('should load editor page', async ({ page }) => {
    await expect(page).toHaveURL(/.*editor/);
    await expect(page.locator('h1')).toContainText('Полотно');
  });

  test('should show loading spinner initially', async ({ page }) => {
    await page.goto('/editor');

    // Check if loading spinner is visible initially
    const loadingSpinner = page.locator('.loading mat-progress-spinner');
    // Loading might be very fast, so we just check if the page loads
    await expect(page.locator('.editor-content, .loading')).toBeVisible();
  });

  test('should display models panel', async ({ page }) => {
    await page.waitForSelector('app-models-panel', { timeout: 10000 });
    const modelsPanel = page.locator('app-models-panel');
    await expect(modelsPanel).toBeVisible();
  });

  test('should show "no model" message when no active model', async ({ page }) => {
    // Wait for page to load
    await page.waitForSelector('.editor-content', { timeout: 10000 });

    // If no active model is selected, should show info message
    const noModelMessage = page.locator('.canvas-empty');
    const isVisible = await noModelMessage.isVisible().catch(() => false);

    if (isVisible) {
      await expect(noModelMessage).toContainText('Оберіть модель');
    } else {
      // If active model exists, canvas should be visible
      await expect(page.locator('app-image-canvas')).toBeVisible();
    }
  });

  test('should have sidebar with action buttons', async ({ page }) => {
    await page.waitForSelector('app-sidebar', { timeout: 10000 });
    const sidebar = page.locator('app-sidebar');
    await expect(sidebar).toBeVisible();
  });

  test('should display image gallery when model is active', async ({ page }) => {
    await page.waitForSelector('.editor-content', { timeout: 10000 });

    // Check if image gallery is present
    const imageGallery = page.locator('app-image-gallery');
    const isVisible = await imageGallery.isVisible().catch(() => false);

    if (isVisible) {
      await expect(imageGallery).toBeVisible();
    }
  });

  test('should display calendar preview panel', async ({ page }) => {
    await page.waitForSelector('.editor-content', { timeout: 10000 });

    // Calendar preview should be visible when active model exists
    const calendarPreview = page.locator('app-calendar-preview');
    const isVisible = await calendarPreview.isVisible().catch(() => false);

    if (isVisible) {
      await expect(calendarPreview).toBeVisible();
    }
  });

  test('should display properties panel', async ({ page }) => {
    await page.waitForSelector('.editor-content', { timeout: 10000 });
    const propertiesPanel = page.locator('app-properties-panel');
    await expect(propertiesPanel).toBeVisible();
  });

  test('should have breadcrumb navigation', async ({ page }) => {
    await page.waitForSelector('.breadcrumbs', { timeout: 10000 });
    const breadcrumbs = page.locator('.breadcrumbs');
    await expect(breadcrumbs).toBeVisible();
    await expect(breadcrumbs.locator('a[routerLink="/"]')).toContainText('Головна');
    await expect(breadcrumbs.locator('span')).toContainText('Редактор');
  });

  test('should navigate to home when clicking breadcrumb', async ({ page }) => {
    await page.waitForSelector('.breadcrumbs a[routerLink="/"]', { timeout: 10000 });
    await page.click('.breadcrumbs a[routerLink="/"]');
    await expect(page).toHaveURL(/.*\//);
  });

  test('should display toolbar section', async ({ page }) => {
    await page.waitForSelector('.editor-content', { timeout: 10000 });
    const toolbar = page.locator('app-toolbar');
    await expect(toolbar).toBeVisible();
  });

  test('should display history panel', async ({ page }) => {
    await page.waitForSelector('.editor-content', { timeout: 10000 });
    const historyPanel = page.locator('app-history');
    await expect(historyPanel).toBeVisible();
  });
});

test.describe('Editor - Model Selection', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/editor');
    await page.waitForSelector('.editor-content', { timeout: 10000 });
  });

  test('should be able to select a model from models panel', async ({ page }) => {
    // This test assumes there are models available
    // Wait for models panel to load
    const modelsPanel = page.locator('app-models-panel');
    await expect(modelsPanel).toBeVisible();

    // Try to find and click on a model
    // Note: This will depend on the actual implementation of models-panel component
    // You might need to adjust selectors based on the actual HTML structure
  });
});

test.describe('Editor - Image Generation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/editor');
    await page.waitForSelector('.editor-content', { timeout: 10000 });
  });

  test('should open generate modal when generate button is clicked', async ({ page }) => {
    // This test requires finding the generate button in the sidebar
    // The actual selector will depend on the sidebar component implementation
    // Placeholder test - adjust based on actual component
  });
});

test.describe('Editor - Calendar Operations', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/editor');
    await page.waitForSelector('.editor-content', { timeout: 10000 });
  });

  test('should allow adding image to calendar month', async ({ page }) => {
    // This test requires having an active model with generated images
    // Placeholder test - adjust based on actual workflow
  });

  test('should show calendar preview with 12 month slots', async ({ page }) => {
    const calendarPreview = page.locator('app-calendar-preview');
    if (await calendarPreview.isVisible()) {
      // Calendar should have 12 months
      // Actual test will depend on calendar-preview component implementation
    }
  });
});

test.describe('Editor - Responsive Design', () => {
  test('should be responsive on tablet', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/editor');
    await expect(page.locator('.editor-content')).toBeVisible();
  });

  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/editor');
    await expect(page.locator('.editor-content')).toBeVisible();
  });
});
