# Робоча папка QA Engineer Calendary

## Про роль

QA Engineer відповідає за забезпечення якості продукту через тестування, автоматизацію, виявлення багів, підтримку якості релізів та співпрацю з командою.

---

## Основні обов'язки

### 1. Manual Testing
- Functional testing нових фіч
- Regression testing після змін
- Exploratory testing
- UAT (User Acceptance Testing) підготовка

### 2. Test Automation
- Написання E2E тестів
- Integration tests
- Підтримка test frameworks
- CI/CD integration

### 3. Bug Management
- Знаходження та документування багів
- Prioritization багів
- Regression verification
- Communication з розробниками

### 4. Quality Assurance
- Test plan creation
- Test case management
- Metrics tracking (bug density, test coverage)
- Release quality gates

### 5. Performance Testing
- Load testing критичних endpoints
- Performance regression detection
- Mobile performance (якщо є мобільний додаток)

---

## Testing Strategy

### Test Pyramid

```
        /\
       /  \  E2E Tests (10%)
      /____\  - User flows
     /      \  - Critical paths
    /________\ Integration Tests (30%)
   /          \ - API tests
  /____________\ - Component integration
 /              \ Unit Tests (60%)
/________________\ - Business logic
                    - Utilities
```

### Test Types

**1. Unit Tests** (Відповідальність: Developers, Review: QA)
- Business logic
- Utility functions
- Services

**2. Integration Tests** (Відповідальність: Developers + QA)
- API endpoints
- Database interactions
- External service mocks

**3. E2E Tests** (Відповідальність: QA)
- User flows
- Critical business scenarios
- Cross-browser testing

**4. Manual Testing** (Відповідальність: QA)
- Exploratory testing
- Usability testing
- Edge cases

---

## Test Planning

### Test Plan Template

```markdown
# Test Plan: AI Image Generation Feature (US-101)

## Objective
Verify that users can successfully generate artistic images using AI with 5 different styles.

## Scope
- In scope: Image upload, style selection, generation, preview, apply
- Out of scope: Payment flow (separate feature)

## Test Environment
- Staging: https://staging.calendary.com
- Test Data: 10 sample images (various formats, sizes)

## Entry Criteria
- Feature deployed to staging
- API key configured
- Test images uploaded

## Exit Criteria
- All high/critical bugs fixed
- 90%+ test cases passed
- Performance acceptable (<60s generation)

## Test Cases
See Test Cases section below

## Risks
- External API (Replicate) might be unavailable
- Mitigation: Mock API for critical tests

## Schedule
- Test execution: 2 days
- Regression: 1 day
```

### Test Case Template

```markdown
## TC-101: Generate Watercolor Style Image

**Priority:** High
**Type:** Functional

**Preconditions:**
- User logged in
- At least one image uploaded

**Steps:**
1. Navigate to Calendar Editor
2. Click on uploaded image
3. Click "AI Styles" button
4. Select "Watercolor" style
5. Wait for generation to complete

**Expected Result:**
- Loading indicator appears
- Within 60 seconds, generated image is displayed
- Image has watercolor artistic style
- "Apply" and "Regenerate" buttons visible

**Actual Result:**
[To be filled during testing]

**Status:** ⏳ Pending / ✅ Passed / ❌ Failed

**Environment:** Staging
**Tested By:** [Name]
**Date:** [Date]
```

---

## Manual Testing Checklist

### Feature Testing: AI Image Generation

**Functionality:**
- [ ] User can select each of 5 styles (watercolor, cartoon, oil, sketch, modern)
- [ ] Loading indicator appears during generation
- [ ] Progress is shown (if API supports)
- [ ] Generated image displays correctly
- [ ] User can regenerate with different style
- [ ] User can apply generated image to calendar
- [ ] Original image is preserved (can revert)

**Edge Cases:**
- [ ] Very small image (100x100)
- [ ] Very large image (5000x5000)
- [ ] Non-standard formats (HEIC, WebP)
- [ ] Corrupted image file
- [ ] Network interruption during generation
- [ ] Multiple generation requests simultaneously

**Error Handling:**
- [ ] API returns 500 error
- [ ] API timeout (>60s)
- [ ] Invalid API key
- [ ] Rate limiting reached
- [ ] Clear error messages shown to user

**UI/UX:**
- [ ] All buttons properly labeled
- [ ] Loading states are clear
- [ ] Error messages are user-friendly
- [ ] Responsive design (mobile, tablet, desktop)
- [ ] Accessibility (keyboard navigation, screen readers)

**Performance:**
- [ ] Generation completes within 60 seconds
- [ ] No UI freezing during generation
- [ ] Image preview loads quickly

**Security:**
- [ ] User can only access their own images
- [ ] No API keys exposed in frontend
- [ ] Proper input validation

**Browser Compatibility:**
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)

---

## Test Automation

### E2E Tests (Playwright/Cypress)

**Setup Playwright:**

```typescript
// tests/e2e/ai-generation.spec.ts
import { test, expect } from '@playwright/test';

test.describe('AI Image Generation', () => {
  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('https://staging.calendary.com/login');
    await page.fill('[name="email"]', 'test@example.com');
    await page.fill('[name="password"]', 'password123');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');
  });

  test('should generate watercolor style image', async ({ page }) => {
    // Navigate to editor
    await page.goto('/calendar/editor');

    // Upload image
    const fileInput = await page.locator('input[type="file"]');
    await fileInput.setInputFiles('./test-data/sample-image.jpg');

    // Wait for upload
    await page.waitForSelector('[data-testid="uploaded-image"]');

    // Click image
    await page.click('[data-testid="uploaded-image"]');

    // Open AI styles
    await page.click('[data-testid="ai-styles-button"]');

    // Select watercolor
    await page.click('[data-testid="style-watercolor"]');

    // Wait for generation (max 60s)
    await page.waitForSelector('[data-testid="generated-image"]', { timeout: 60000 });

    // Verify generated image exists
    const generatedImage = await page.locator('[data-testid="generated-image"]');
    await expect(generatedImage).toBeVisible();

    // Verify apply button
    const applyButton = await page.locator('[data-testid="apply-button"]');
    await expect(applyButton).toBeEnabled();
  });

  test('should show error when API fails', async ({ page }) => {
    // Mock API to return error
    await page.route('**/api/v1/ai/generate', route => {
      route.fulfill({
        status: 500,
        body: JSON.stringify({ message: 'Service unavailable' })
      });
    });

    await page.goto('/calendar/editor');
    // ... trigger generation

    // Verify error message
    const errorMessage = await page.locator('[data-testid="error-message"]');
    await expect(errorMessage).toContainText('unavailable');
  });

  test('should allow regeneration', async ({ page }) => {
    await page.goto('/calendar/editor');
    // ... generate first image

    await page.click('[data-testid="regenerate-button"]');

    // Select different style
    await page.click('[data-testid="style-cartoon"]');

    await page.waitForSelector('[data-testid="generated-image"]', { timeout: 60000 });

    // Verify new image is different
    // (would need to compare image URLs or checksums)
  });
});

// Run tests
// npx playwright test
```

### API Testing (Postman/Newman)

**Collection: Calendary API Tests**

```json
{
  "info": {
    "name": "Calendary API Tests"
  },
  "item": [
    {
      "name": "AI Generation - Success",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/v1/ai/generate",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{accessToken}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"imageId\": \"{{imageId}}\",\n  \"imageUrl\": \"https://example.com/image.jpg\",\n  \"style\": \"watercolor\"\n}"
        }
      },
      "event": [
        {
          "listen": "test",
          "script": {
            "exec": [
              "pm.test('Status code is 200', function() {",
              "  pm.response.to.have.status(200);",
              "});",
              "",
              "pm.test('Response has generatedImageUrl', function() {",
              "  var jsonData = pm.response.json();",
              "  pm.expect(jsonData).to.have.property('generatedImageUrl');",
              "});",
              "",
              "pm.test('Response time is less than 60000ms', function() {",
              "  pm.expect(pm.response.responseTime).to.be.below(60000);",
              "});"
            ]
          }
        }
      ]
    },
    {
      "name": "AI Generation - Invalid Style",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/v1/ai/generate",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"imageId\": \"{{imageId}}\",\n  \"imageUrl\": \"https://example.com/image.jpg\",\n  \"style\": \"invalid_style\"\n}"
        }
      },
      "event": [
        {
          "listen": "test",
          "script": {
            "exec": [
              "pm.test('Status code is 400', function() {",
              "  pm.response.to.have.status(400);",
              "});",
              "",
              "pm.test('Error message present', function() {",
              "  var jsonData = pm.response.json();",
              "  pm.expect(jsonData).to.have.property('errors');",
              "});"
            ]
          }
        }
      ]
    }
  ]
}
```

**Run via CLI:**
```bash
newman run calendary-api-tests.json -e staging.json --reporters cli,html
```

---

## Bug Management

### Bug Report Template

```markdown
# BUG-101: AI generation fails for HEIC images

**Priority:** High
**Severity:** Major
**Status:** Open
**Assignee:** Backend Dev
**Found in:** Sprint 15
**Environment:** Staging

## Description
When user uploads HEIC format image and tries to generate AI style, the generation fails with "Unsupported format" error.

## Steps to Reproduce
1. Login to staging
2. Navigate to Calendar Editor
3. Upload HEIC image (attached: test-image.heic)
4. Click AI Styles
5. Select any style
6. Observe error

## Expected Behavior
- HEIC should be converted to JPEG before sending to AI API
- OR: Clear message "Please upload JPEG/PNG format"

## Actual Behavior
- Generic error "Generation failed"
- No indication that format is the issue

## Screenshots
[Attach screenshot]

## Logs
```
ERROR: Replicate API returned 400: Unsupported image format
```

## Impact
- Users with iPhone (HEIC default format) cannot use AI feature

## Proposed Fix
- Convert HEIC to JPEG on backend before API call
- Add validation on frontend to prevent HEIC upload
```

### Bug Severity Levels

**Critical:**
- Application crashes
- Data loss
- Security vulnerability
- Payment processing failure

**High:**
- Major feature unusable
- Workaround exists but difficult
- Affects many users

**Medium:**
- Feature partially works
- Easy workaround exists
- Affects some users

**Low:**
- Minor UI glitch
- Cosmetic issue
- Affects very few users

### Bug Workflow

```
New → In Progress → Fixed → Ready for Testing → Verified → Closed
                                               ↓
                                            Reopened (if not fixed)
```

---

## Test Reporting

### Daily Test Report

```markdown
## QA Daily Report - Sprint 15, Day 5

**Date:** 2024-01-20
**Tester:** Ivan

### Testing Progress
- Stories tested: US-101, US-102
- Test cases executed: 25
- Test cases passed: 22
- Test cases failed: 3

### Bugs Found
- BUG-101: AI generation fails for HEIC (High)
- BUG-102: Loading spinner doesn't show (Low)
- BUG-103: Regenerate button disabled after error (Medium)

### Blockers
- None

### Tomorrow's Plan
- Continue US-103 testing
- Regression test US-101 after fix
```

### Sprint Test Summary

```markdown
## Sprint 15 - QA Summary

### Test Execution
- Total test cases: 120
- Passed: 110 (92%)
- Failed: 5 (4%)
- Blocked: 5 (4%)

### Bug Statistics
- Bugs found: 15
- Critical: 0
- High: 3
- Medium: 7
- Low: 5
- Bugs fixed: 12
- Bugs remaining: 3 (all Low priority)

### Test Coverage
- Features tested: 8/8 (100%)
- Regression coverage: 85%
- Automation coverage: 60%

### Quality Gates
- ✅ No critical bugs
- ✅ All high bugs fixed
- ✅ >90% test pass rate
- ✅ Performance within SLA

### Recommendation
**GO for Release** ✅
```

---

## Regression Testing

### Regression Test Suite

**Frequency:** Before every release

**Core Flows:**
1. **User Registration & Login**
   - Register new account
   - Login with email/password
   - Logout

2. **Calendar Creation**
   - Select template
   - Upload images
   - Add text
   - Save draft

3. **AI Features** (if applicable)
   - Generate artistic style
   - Apply to calendar

4. **Order & Checkout**
   - Add to cart
   - Enter delivery details
   - Select Nova Poshta branch
   - Complete payment (MonoBank)

5. **Order Tracking**
   - View order history
   - Check order status
   - Download PDF (digital orders)

**Automation:**
- 70% of regression suite automated (Playwright)
- Runs on every merge to `develop`
- Nightly runs on `main`

---

## Performance Testing

### Load Testing (k6)

```javascript
// load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 10 },  // Ramp up to 10 users
    { duration: '5m', target: 10 },  // Stay at 10 users
    { duration: '2m', target: 50 },  // Ramp up to 50 users
    { duration: '5m', target: 50 },  // Stay at 50 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'], // 95% requests < 2s
    http_req_failed: ['rate<0.01'],    // Error rate < 1%
  },
};

export default function () {
  // Test AI generation endpoint
  let payload = JSON.stringify({
    imageId: 'test-id',
    imageUrl: 'https://example.com/image.jpg',
    style: 'watercolor'
  });

  let params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer test-token'
    },
  };

  let res = http.post('https://staging.calendary.com/api/v1/ai/generate', payload, params);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 60s': (r) => r.timings.duration < 60000,
  });

  sleep(1);
}
```

**Run:**
```bash
k6 run load-test.js
```

### Performance Benchmarks

| Endpoint | p50 | p95 | p99 | Target |
|----------|-----|-----|-----|--------|
| GET /api/calendars | 150ms | 300ms | 500ms | <500ms |
| POST /api/ai/generate | 30s | 55s | 60s | <60s |
| POST /api/orders | 200ms | 400ms | 600ms | <1s |
| GET /api/templates | 50ms | 100ms | 150ms | <200ms |

---

## Test Environments

### Environments

**1. Local Development**
- URL: http://localhost:4200
- Database: Local MSSQL
- Purpose: Developer testing

**2. Staging**
- URL: https://staging.calendary.com
- Database: Staging MSSQL (DigitalOcean)
- Purpose: QA testing, UAT
- Deployment: Auto-deploy from `develop` branch

**3. Production**
- URL: https://calendary.com
- Database: Production MSSQL
- Purpose: Live users
- Deployment: Manual deploy from `main` branch

### Test Data Management

**Staging Test Accounts:**
```
User 1 (Regular):
- Email: test.user@calendary.com
- Password: Test123!

User 2 (With orders):
- Email: test.orders@calendary.com
- Password: Test123!

Admin:
- Email: admin@calendary.com
- Password: Admin123!
```

**Test Images:**
- Location: `/test-data/images/`
- Formats: JPEG, PNG, WebP
- Sizes: 100KB - 5MB

---

## Quality Metrics

### Tracking

**Bug Metrics:**
- Bug density: Bugs per feature
- Defect removal efficiency: (Bugs found pre-release) / (Total bugs)
- Escaped defects: Bugs found in production

**Test Metrics:**
- Test coverage: % of features tested
- Automation coverage: % automated tests
- Test execution rate: Tests run per sprint
- Pass rate: % tests passed

**Target KPIs:**
- Automation coverage: >70%
- Test pass rate: >90%
- Escaped defects: <5 per release
- Critical bugs in production: 0

---

## Tools

### Testing Tools
- **Playwright** - E2E testing
- **Postman/Newman** - API testing
- **k6** - Load testing
- **xUnit** - Unit testing (backend)
- **Jasmine/Karma** - Unit testing (frontend)

### Bug Tracking
- **Jira** - Bug management
- **Azure DevOps** - Test case management

### CI/CD
- **GitHub Actions** - Automated test runs
- **SonarQube** - Code quality analysis

---

## Best Practices

### Testing Principles

1. **Test Early, Test Often**
   - Start testing as soon as feature is available
   - Don't wait until end of sprint

2. **Think Like a User**
   - Test realistic scenarios
   - Consider different user types

3. **Be Thorough**
   - Don't just test happy path
   - Test edge cases, error scenarios

4. **Document Everything**
   - Test cases should be reproducible
   - Bug reports should be clear

5. **Automate Wisely**
   - Automate stable, repetitive tests
   - Keep critical flows automated
   - Don't over-automate

6. **Communicate**
   - Daily standup updates
   - Immediately report blockers
   - Collaborate with developers

---

## Release Checklist

**1 Week Before Release:**
- [ ] Review release scope
- [ ] Update test cases
- [ ] Prepare test data
- [ ] Schedule regression testing

**3 Days Before:**
- [ ] Execute full regression suite
- [ ] Verify all high/critical bugs fixed
- [ ] Performance testing
- [ ] Security scan

**1 Day Before:**
- [ ] Smoke test on staging
- [ ] Verify deployment scripts
- [ ] Backup verification
- [ ] Rollback plan ready

**Release Day:**
- [ ] Deploy to production
- [ ] Smoke test production
- [ ] Monitor logs for errors
- [ ] Verify critical flows

**Post-Release:**
- [ ] Monitor production for 24h
- [ ] Document any issues
- [ ] Sprint retrospective input

---

## Useful Resources

### Learning
- "Lessons Learned in Software Testing" - Cem Kaner
- ISTQB Certification
- Test Automation University (free)

### Communities
- Ministry of Testing
- Software Testing Help
- r/QualityAssurance

---

## Контакти

- **Tech Lead:** [ім'я]
- **Scrum Master:** [ім'я]
- **Developers:** [імена]
- **Product Owner:** [ім'я]
