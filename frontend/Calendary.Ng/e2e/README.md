# E2E Tests for Calendary Editor

End-to-end tests for the Calendary application using Playwright.

## Test Files

- **editor.spec.ts** - Tests for /editor page functionality
  - Page loading and basic UI elements
  - Model selection
  - Image generation workflow
  - Responsive design tests

- **calendar-generation.spec.ts** - Tests for calendar generation workflow
  - Full workflow from image generation to PDF export
  - Image assignment to calendar months
  - Calendar preview
  - Month selector dialog
  - PDF generation
  - Visual regression tests

## Running Tests

### Run all tests (headless mode)
```bash
npm run test:e2e
```

### Run tests with UI (interactive mode)
```bash
npm run test:e2e:ui
```

### Run tests in headed mode (see browser)
```bash
npm run test:e2e:headed
```

### Run tests in specific browser
```bash
npm run test:e2e:chromium
npm run test:e2e:firefox
npm run test:e2e:webkit
```

### Debug tests
```bash
npm run test:e2e:debug
```

## Prerequisites

Before running tests:

1. Install dependencies:
   ```bash
   npm install
   ```

2. Install Playwright browsers:
   ```bash
   npx playwright install
   ```

3. Make sure the Angular dev server is running:
   ```bash
   npm start
   ```

   Or configure `playwright.config.ts` to start the server automatically (already configured).

## Configuration

Test configuration is in `playwright.config.ts` in the root of the project.

Key settings:
- **baseURL**: http://localhost:4200
- **Test directory**: ./e2e
- **Browsers**: Chromium, Firefox, WebKit
- **Screenshots**: On failure only
- **Trace**: On first retry

## Authentication

Currently, authentication is not implemented in the tests. You may need to:

1. Add authentication setup in `beforeEach` hooks
2. Use Playwright's storage state to save authenticated sessions
3. Or mock authentication for testing purposes

## TODO

The following test scenarios are outlined but need full implementation:

1. **Authentication flow**
   - Login before accessing /editor
   - Store authentication state

2. **Model selection**
   - Click on specific model
   - Verify active model changes

3. **Image generation**
   - Open generate modal
   - Fill in prompt
   - Wait for image generation
   - Verify image appears in gallery

4. **Calendar operations**
   - Add image to calendar month
   - Swap months
   - Clear assignments
   - Detect duplicates

5. **PDF generation**
   - Complete calendar with all 12 months
   - Generate and download PDF
   - Verify PDF content

6. **Visual regression**
   - Update baseline screenshots
   - Compare against baselines

## Best Practices

1. Use `data-testid` attributes in components for reliable selectors
2. Wait for elements to be visible before interacting
3. Keep tests independent and isolated
4. Use descriptive test names
5. Add comments for complex workflows
6. Update tests when UI changes

## CI/CD Integration

Tests are configured to run in CI/CD pipelines with:
- Reduced workers (1 worker on CI)
- 2 retries on failure
- HTML report generation

## Visual Regression Testing

Visual regression tests take screenshots and compare them to baselines.

To update baseline screenshots:
```bash
npx playwright test --update-snapshots
```

## Troubleshooting

### Tests failing due to timeouts
- Increase timeout in test configuration
- Check if dev server is running
- Verify network connectivity

### Screenshots don't match
- Update baselines if intentional UI changes were made
- Check for animation timing issues
- Verify viewport size consistency

### Authentication issues
- Implement proper auth setup in beforeEach
- Use storage state for persistent auth
- Check API endpoints

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Angular Testing Guide](https://angular.io/guide/testing)
