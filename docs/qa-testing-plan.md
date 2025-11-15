# QA Testing Plan - Calendary

> **Версія документу:** 1.0
> **Дата створення:** 2025-11-15
> **QA Tech Lead:** AI Team
> **Статус:** Active

---

## Зміст

1. [Огляд проєкту](#огляд-проєкту)
2. [Цілі та завдання QA](#цілі-та-завдання-qa)
3. [Стратегія тестування](#стратегія-тестування)
4. [Типи тестування](#типи-тестування)
5. [Тестові сценарії](#тестові-сценарії)
6. [Критерії якості](#критерії-якості)
7. [Тестове оточення](#тестове-оточення)
8. [Інструменти](#інструменти)
9. [Процес тестування](#процес-тестування)
10. [Чек-лист перед релізом](#чек-лист-перед-релізом)
11. [Метрики та KPI](#метрики-та-kpi)

---

## Огляд проєкту

**Calendary** - онлайн-платформа для створення персоналізованих календарних постерів з можливістю замовлення друку та доставки.

### Ключові функції:
- Редактор календарів з вибором шаблонів
- Завантаження та обробка зображень
- AI-генерація художніх стилів (Replicate AI)
- Інтеграція оплати (MonoBank API)
- Інтеграція доставки (Nova Poshta API)
- Генерація PDF для друку
- Адміністративна панель для обробки замовлень
- Email/SMS повідомлення

### Технологічний стек:
- **Backend:** .NET 9.0, ASP.NET Core Web API, MS SQL Server, EF Core
- **Frontend Customer:** React/Next.js (планується)
- **Frontend Admin:** Angular 18+
- **Message Queue:** RabbitMQ
- **External Services:** MonoBank, Nova Poshta, SendGrid, SmsClub, Replicate AI

---

## Цілі та завдання QA

### Основні цілі:

1. **Забезпечення якості продукту** - перевірка відповідності вимогам
2. **Виявлення дефектів** - знаходження багів на ранніх стадіях
3. **Перевірка безпеки** - захист даних користувачів та платіжної інформації
4. **Тестування продуктивності** - стабільність під навантаженням
5. **Покращення UX** - зручність використання
6. **Мінімізація ризиків** - попередження критичних помилок на production

### Ключові завдання:

- Створення та підтримка тест-кейсів
- Виконання функціонального тестування
- Автоматизація регресійних тестів
- Тестування інтеграцій
- Навантажувальне тестування
- Security тестування
- UAT координація

---

## Стратегія тестування

### Test Pyramid

```
           /\
          /  \ E2E Tests (10%)
         /____\  - Критичні user flows
        /      \ - End-to-end сценарії
       /________\ Integration Tests (30%)
      /          \ - API тестування
     /____________\ - Інтеграції
    /              \ Unit Tests (60%)
   /________________\ - Бізнес логіка
                       - Сервіси та утиліти
```

### Підхід до тестування:

**Risk-Based Testing** - фокус на критичних функціях:
1. **Критична важливість:**
   - Оплата (MonoBank integration)
   - Обробка замовлень
   - Генерація PDF
   - Створення ТТН

2. **Висока важливість:**
   - Реєстрація/авторизація
   - Редактор календарів
   - AI генерація зображень
   - Email/SMS notifications

3. **Середня важливість:**
   - Профіль користувача
   - Історія замовлень
   - Адмін панель (аналітика)

### Shift-Left Testing

- Залучення QA на етапі планування
- Code review з точки зору testability
- Unit tests пишуть розробники
- Ранні тести на feature branches

---

## Типи тестування

### 1. Функціональне тестування

#### 1.1 Smoke Testing
**Коли:** Після кожного деплою
**Тривалість:** 15-30 хв
**Scope:**
- Основні сторінки відкриваються
- API health check
- Авторизація працює
- База даних доступна

#### 1.2 Regression Testing
**Коли:** Перед кожним релізом
**Тривалість:** 4-8 годин
**Scope:**
- Всі критичні user flows
- Основні функції
- Інтеграції

#### 1.3 Exploratory Testing
**Коли:** Для нових фіч
**Тривалість:** 2-4 години на фічу
**Scope:**
- Нестандартні сценарії
- Edge cases
- Usability issues

### 2. Non-Functional Testing

#### 2.1 Performance Testing
**Інструменти:** k6, Apache JMeter
**Метрики:**
- Response time: p50 < 200ms, p95 < 500ms, p99 < 1s
- Throughput: 100+ req/s
- Error rate: < 0.1%

**Сценарії:**
- Load testing: 100 concurrent users
- Stress testing: 500+ concurrent users
- Spike testing: різкі стрибки навантаження

#### 2.2 Security Testing
**Інструменти:** OWASP ZAP, Burp Suite
**Перевірки:**
- SQL Injection
- XSS (Cross-Site Scripting)
- CSRF (Cross-Site Request Forgery)
- Broken Authentication
- Sensitive Data Exposure
- Security Misconfiguration

#### 2.3 Compatibility Testing
**Браузери:**
- Chrome (latest, latest-1)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

**Mobile:**
- iOS Safari (iOS 15+)
- Chrome Mobile (Android 10+)

**Резолюції:**
- Desktop: 1920x1080, 1366x768
- Tablet: 768x1024
- Mobile: 375x667, 414x896

### 3. API Testing

**Інструменти:** Postman, Newman, xUnit
**Scope:**
- Всі REST endpoints
- Request/Response validation
- Error handling
- Authentication/Authorization
- Rate limiting

### 4. Integration Testing

**Критичні інтеграції:**

#### 4.1 MonoBank API
- Створення invoice
- Webhook обробка
- Статуси платежів
- Error scenarios

#### 4.2 Nova Poshta API
- Розрахунок вартості доставки
- Пошук відділень
- Створення ТТН
- Відстеження посилки

#### 4.3 Replicate AI
- Генерація зображень
- Різні стилі
- Timeout handling
- Rate limits

#### 4.4 Email/SMS
- SendGrid template rendering
- SmsClub delivery
- Статуси відправки

---

## Тестові сценарії

### Критичний User Flow 1: Створення та замовлення календаря

#### TC-001: Повний цикл замовлення (Happy Path)

**Пріоритет:** Критичний
**Тип:** E2E
**Очікуваний час:** 5-7 хв

**Передумови:**
- Користувач не авторизований
- Тестові дані: valid email, phone, photos

**Кроки:**

1. **Реєстрація**
   - Відкрити головну сторінку
   - Клікнути "Реєстрація"
   - Заповнити форму (email, password, phone)
   - Підтвердити email (отримати код)
   - ✅ Користувач створений

2. **Вибір шаблону**
   - Перейти в каталог
   - Вибрати шаблон календаря
   - Клікнути "Створити календар"
   - ✅ Відкрився редактор

3. **Редагування календаря**
   - Завантажити 12 фото (по одному на місяць)
   - Додати текст/дати (день народження, річниця)
   - Вибрати мову (Українська)
   - Вибрати перший день тижня (Понеділок)
   - Попередній перегляд
   - ✅ Дизайн збережено

4. **AI генерація (опціонально)**
   - Вибрати фото
   - Застосувати AI стиль (акварель)
   - Дочекатись генерації (< 60s)
   - Застосувати згенероване зображення
   - ✅ AI зображення додано

5. **Додавання в кошик**
   - Клікнути "Додати в кошик"
   - Вибрати кількість (1 шт)
   - ✅ Календар в кошику

6. **Checkout**
   - Відкрити кошик
   - Клікнути "Оформити замовлення"
   - Заповнити дані доставки (ім'я, адреса)
   - Вибрати відділення НП (autocomplete)
   - ✅ Розрахована вартість доставки
   - Застосувати промо-код (якщо є)
   - ✅ Знижка застосована

7. **Оплата**
   - Клікнути "Перейти до оплати"
   - Редірект на MonoBank
   - Ввести тестові дані картки
   - Підтвердити оплату
   - ✅ Редірект назад з success статусом

8. **Підтвердження**
   - ✅ Замовлення створено (Order ID отримано)
   - ✅ Email підтвердження отримано
   - ✅ SMS підтвердження отримано
   - ✅ Статус "Оплачено" в особистому кабінеті

**Очікуваний результат:**
- Замовлення успішно створено
- Оплата пройшла
- Користувач отримав підтвердження
- В адмін панелі замовлення з'явилось

**Автоматизація:** Playwright/Cypress (пріоритет високий)

---

#### TC-002: Створення замовлення (Guest User)

**Передумови:**
- Користувач не зареєстрований

**Кроки:**
1. Вибрати шаблон
2. Створити календар (без реєстрації)
3. При checkout - ввести email/phone
4. Опція "Створити акаунт" (checkbox)
5. Завершити оплату

**Очікуваний результат:**
- Замовлення створено без реєстрації
- Якщо checkbox вибрано - акаунт створюється автоматично
- Email з паролем відправлено

---

### Критичний User Flow 2: Обробка замовлення (Admin)

#### TC-010: Обробка оплаченого замовлення

**Роль:** Admin
**Пріоритет:** Критичний

**Передумови:**
- Є оплачене замовлення (Order ID)
- Admin авторизований

**Кроки:**

1. **Перегляд замовлення**
   - Логін в Admin Panel
   - Відкрити "Замовлення"
   - Знайти замовлення (фільтр за статусом "Оплачено")
   - ✅ Замовлення відображається

2. **Перевірка деталей**
   - Відкрити замовлення
   - Перевірити дані клієнта
   - Перевірити дані доставки
   - Переглянути PDF (попередній перегляд)
   - ✅ Всі дані коректні

3. **Відправка на друк**
   - Клікнути "Надіслати на друк"
   - Вибрати друкарню (з списку)
   - ✅ PDF відправлено на email друкарні
   - ✅ Статус змінено на "Відправлено на друк"

4. **Створення ТТН**
   - Після підтвердження друку → "Створити ТТН"
   - Автозаповнення даних (з замовлення)
   - Перевірка даних НП
   - Клікнути "Створити"
   - ✅ ТТН створена (отримано номер)
   - ✅ Статус змінено на "Відправлено"
   - ✅ Email з ТТН відправлено клієнту

5. **Відстеження**
   - Перевірити статус посилки (Nova Poshta API)
   - ✅ Статус оновлюється автоматично
   - Коли доставлено → статус "Доставлено"

**Очікуваний результат:**
- Замовлення оброблено успішно
- Клієнт отримав всі повідомлення
- Статус відстежується

---

### API Testing Scenarios

#### API-001: POST /api/auth/register

**Тип:** Positive

```json
POST /api/auth/register
{
  "email": "test@example.com",
  "password": "SecurePass123!",
  "phone": "+380501234567",
  "firstName": "Іван",
  "lastName": "Петренко"
}

Expected: 201 Created
Response:
{
  "userId": "uuid",
  "email": "test@example.com",
  "message": "Verification email sent"
}
```

**Перевірки:**
- Status code: 201
- User створений в БД
- Email відправлено
- Password захешований (bcrypt)
- JWT token не повертається (потрібна верифікація)

---

#### API-002: POST /api/auth/register (Invalid Email)

**Тип:** Negative

```json
POST /api/auth/register
{
  "email": "invalid-email",
  "password": "SecurePass123!",
  "phone": "+380501234567"
}

Expected: 400 Bad Request
Response:
{
  "errors": {
    "email": ["Invalid email format"]
  }
}
```

---

#### API-010: POST /api/calendars

**Тип:** Positive
**Auth:** Required (Bearer token)

```json
POST /api/calendars
Authorization: Bearer {token}
{
  "templateId": "uuid",
  "name": "Сімейний календар 2025",
  "language": "uk",
  "startDayOfWeek": "Monday",
  "images": [
    {
      "month": 1,
      "imageId": "uuid"
    },
    ...
  ],
  "eventDates": [
    {
      "date": "2025-05-15",
      "title": "День народження"
    }
  ]
}

Expected: 201 Created
Response:
{
  "calendarId": "uuid",
  "previewUrl": "https://...",
  "createdAt": "2025-11-15T10:00:00Z"
}
```

**Перевірки:**
- Calendar створений
- Images асоційовані
- Preview згенеровано
- UserId прив'язаний

---

#### API-020: POST /api/payments/create

**Тип:** Integration
**Auth:** Required

```json
POST /api/payments/create
Authorization: Bearer {token}
{
  "orderId": "uuid",
  "amount": 35000,
  "currency": "UAH",
  "returnUrl": "https://calendary.com/orders/success",
  "webhookUrl": "https://calendary.com/api/webhooks/monobank"
}

Expected: 200 OK
Response:
{
  "invoiceId": "mono-invoice-id",
  "pageUrl": "https://pay.monobank.ua/...",
  "expiresAt": "2025-11-15T11:00:00Z"
}
```

**Перевірки:**
- MonoBank API викликано
- Invoice створено
- PageUrl валідний
- PaymentInfo збережено в БД
- Webhook URL зареєстровано

---

#### API-030: POST /api/webhooks/monobank

**Тип:** Integration (Webhook)
**Auth:** MonoBank signature

```json
POST /api/webhooks/monobank
X-Signature: {monobank-signature}
{
  "invoiceId": "mono-invoice-id",
  "status": "success",
  "amount": 35000,
  "createdDate": "2025-11-15T10:30:00Z"
}

Expected: 200 OK
```

**Перевірки:**
- Signature validated
- Order status updated
- Email/SMS sent
- MonoWebhookEvent збережено
- Idempotency (повторний webhook ignored)

---

#### API-040: POST /api/ai/generate

**Тип:** Integration
**Auth:** Required

```json
POST /api/ai/generate
Authorization: Bearer {token}
{
  "imageId": "uuid",
  "style": "watercolor",
  "prompt": "artistic watercolor style"
}

Expected: 202 Accepted
Response:
{
  "jobId": "uuid",
  "status": "pending",
  "estimatedTime": 60
}
```

**Перевірки:**
- Job створено в БД
- RabbitMQ message відправлено
- Статус "pending"
- Polling endpoint: GET /api/ai/jobs/{jobId}

---

### Integration Testing Scenarios

#### INT-001: MonoBank Payment Flow (Happy Path)

**Компоненти:**
- Frontend → Backend API → MonoBank API → Webhook

**Кроки:**
1. POST /api/payments/create
2. ✅ MonoBank invoice created
3. User redirected to MonoBank page
4. User completes payment (test card)
5. MonoBank sends webhook
6. POST /api/webhooks/monobank received
7. ✅ Order status updated
8. ✅ Email sent
9. ✅ SMS sent

**Автоматизація:** Mock MonoBank responses, test webhook handling

---

#### INT-002: Nova Poshta TTN Creation

**Компоненти:**
- Admin Panel → Backend API → Nova Poshta API

**Кроки:**
1. Admin clicks "Створити ТТН"
2. POST /api/admin/orders/{id}/create-ttn
3. API calls Nova Poshta (InternetDocument/save)
4. ✅ TTN number received
5. ✅ Order updated
6. ✅ Email sent to customer

**Перевірки:**
- Valid API key
- Correct data mapping
- Error handling (invalid address)
- Retry logic

---

#### INT-003: AI Image Generation (Async)

**Компоненти:**
- Frontend → Backend API → RabbitMQ → Consumer → Replicate API

**Кроки:**
1. POST /api/ai/generate
2. Job created, RabbitMQ message sent
3. Consumer picks up job
4. Calls Replicate API (img2img)
5. Poll for completion (webhook or polling)
6. Download generated image
7. Save to storage
8. Update job status
9. Notify frontend (WebSocket/polling)

**Перевірки:**
- Queue processing
- Timeout handling (60s limit)
- Error scenarios (API failure)
- Retry logic

---

### Security Testing Scenarios

#### SEC-001: SQL Injection

**Endpoint:** GET /api/calendars?search={query}

**Test Cases:**
```
1. search=' OR '1'='1
2. search='; DROP TABLE Users; --
3. search=' UNION SELECT * FROM Users --
```

**Expected:** Всі запити безпечні (параметризовані queries)

---

#### SEC-002: XSS (Cross-Site Scripting)

**Endpoint:** POST /api/calendars (eventDates.title)

**Test Case:**
```json
{
  "eventDates": [
    {
      "title": "<script>alert('XSS')</script>"
    }
  ]
}
```

**Expected:**
- Input sanitized
- Output escaped в HTML
- No script execution

---

#### SEC-003: CSRF Protection

**Test Case:**
- Спроба виконати POST /api/orders з іншого домену без CSRF token

**Expected:**
- 403 Forbidden (якщо використовується cookie auth)
- JWT tokens не схильні до CSRF

---

#### SEC-004: Broken Authentication

**Test Cases:**

1. **Weak Password:**
```json
POST /api/auth/register
{
  "password": "123"
}
Expected: 400 Bad Request (password requirements)
```

2. **JWT Expiration:**
```
Use expired token → 401 Unauthorized
```

3. **Token Refresh:**
```
POST /api/auth/refresh-token
{
  "refreshToken": "..."
}
Expected: New access token
```

---

#### SEC-005: Sensitive Data Exposure

**Перевірки:**
- Passwords не повертаються в API responses
- HTTPS всюди (production)
- MonoBank API key не експонується
- No credentials in logs

---

### Performance Testing Scenarios

#### PERF-001: Load Test - API Endpoints

**Інструмент:** k6

**Scenario:**
- 100 concurrent users
- Duration: 10 minutes
- Endpoints:
  - GET /api/calendars (70%)
  - POST /api/calendars (20%)
  - POST /api/orders (10%)

**SLA:**
- Response time p95 < 500ms
- Error rate < 0.1%
- Throughput > 100 req/s

**k6 Script:**
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 50 },
    { duration: '5m', target: 100 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.001'],
  },
};

export default function () {
  const token = 'Bearer ...';

  // 70% GET requests
  let res1 = http.get('https://api.calendary.com/api/calendars', {
    headers: { Authorization: token },
  });
  check(res1, { 'status 200': (r) => r.status === 200 });

  sleep(1);
}
```

---

#### PERF-002: PDF Generation Performance

**Test:**
- Генерація 100 PDF календарів (різні розміри)
- Вимірювання часу

**SLA:**
- A4 calendar: < 5s
- A3 calendar: < 10s
- Memory usage: < 500MB per generation

---

#### PERF-003: AI Generation Throughput

**Test:**
- 50 concurrent AI generation requests

**SLA:**
- Average time: < 45s
- Max time: < 60s
- Queue processing: FIFO
- No timeouts

---

#### PERF-004: Database Query Optimization

**Endpoints to test:**
- GET /api/admin/orders (with filters, pagination)
- GET /api/calendars (user's calendars)

**Metrics:**
- Query execution time < 100ms
- No N+1 queries
- Proper indexing

**Tools:** SQL Server Profiler, EF Core logging

---

## Критерії якості

### Definition of Done (DoD)

Фіча вважається завершеною, якщо:

#### Розробка:
- [ ] Code написаний та reviewed
- [ ] Unit tests покриття > 80%
- [ ] Integration tests написані
- [ ] Code quality (SonarQube) passed
- [ ] No critical/high bugs

#### QA:
- [ ] Functional testing completed
- [ ] Regression tests passed
- [ ] Performance benchmarks met
- [ ] Security scan completed
- [ ] Cross-browser tested (Chrome, Firefox, Safari)
- [ ] Mobile responsive verified

#### Документація:
- [ ] API documentation updated (Swagger)
- [ ] User documentation (якщо потрібно)
- [ ] Release notes prepared

#### Deployment:
- [ ] Deployed to staging
- [ ] Smoke tests passed
- [ ] UAT completed (якщо застосовно)

---

### Entry Criteria (для QA)

QA починає тестування, якщо:
- [ ] Feature deployed на staging/test environment
- [ ] Unit tests passed (CI/CD green)
- [ ] Test data prepared
- [ ] Test cases reviewed
- [ ] API documentation available

### Exit Criteria (для релізу)

Релиз можливий, якщо:
- [ ] 0 Critical bugs
- [ ] 0 High bugs (або всі approved для post-release fix)
- [ ] Medium/Low bugs < 5
- [ ] Regression test suite passed > 95%
- [ ] Performance tests passed
- [ ] Security scan passed
- [ ] Stakeholder approval

---

## Тестове оточення

### Environments

#### 1. Local Development
- **URL:** http://localhost:5000 (API), http://localhost:4200 (Admin)
- **Database:** Local MS SQL Server
- **Purpose:** Developer testing
- **Access:** Developers only

#### 2. Staging
- **URL:** https://staging.calendary.com
- **Database:** Staging MS SQL (DigitalOcean)
- **Purpose:** QA testing, Integration testing, UAT
- **Deployment:** Auto-deploy from `develop` branch
- **Access:** QA, Developers, PO

#### 3. Production
- **URL:** https://calendary.com
- **Database:** Production MS SQL (DigitalOcean)
- **Purpose:** Live users
- **Deployment:** Manual deploy from `main` branch (after approval)
- **Access:** Admins, DevOps

---

### Test Data Management

#### Staging Test Accounts

**Regular Users:**
```
User 1:
- Email: qa.user1@calendary.test
- Password: TestPass123!
- Phone: +380501111111

User 2 (with orders):
- Email: qa.user2@calendary.test
- Password: TestPass123!
- Phone: +380502222222
```

**Admin Users:**
```
Admin:
- Email: qa.admin@calendary.test
- Password: AdminPass123!
- Role: Admin

Marketer:
- Email: qa.marketing@calendary.test
- Password: MarketPass123!
- Role: Marketer
```

#### Test Images
- **Location:** `/test-data/images/`
- **Formats:** JPEG, PNG, WebP
- **Sizes:**
  - Small: 100KB
  - Medium: 500KB - 1MB
  - Large: 5MB
- **Content:** Neutral (no copyright issues)

#### Test Payment Cards (MonoBank Sandbox)
```
Success:
- Card: 5375 4141 0000 0047
- Expiry: 12/26
- CVV: 123

Declined:
- Card: 5375 4141 0000 0005
- Expiry: 12/26
- CVV: 123
```

---

## Інструменти

### Testing Tools

#### Backend Testing
- **xUnit** - Unit testing (.NET)
- **Moq** - Mocking framework
- **FluentAssertions** - Readable assertions
- **Postman/Newman** - API testing
- **k6** - Load/Performance testing

#### Frontend Testing
- **Playwright** - E2E testing (preferred)
- **Cypress** - Alternative E2E tool
- **Jest** - Unit testing (React/Angular)
- **React Testing Library** - Component testing
- **Jasmine/Karma** - Angular testing

#### Security Testing
- **OWASP ZAP** - Security scanning
- **SonarQube** - Code quality + security
- **Snyk** - Dependency vulnerabilities

#### CI/CD
- **GitHub Actions** - Automated testing
- **Docker** - Test environments

#### Bug Tracking
- **GitHub Issues** - Bug management
- **Linear** (optional) - Project management

#### Test Management
- **TestRail** (optional) - Test case management
- **Markdown files** - Lightweight alternative (in repo)

---

## Процес тестування

### 1. Test Planning

**Коли:** Початок спринту
**Учасники:** QA Lead, PO, Tech Lead

**Активності:**
- Review user stories
- Identify testable acceptance criteria
- Estimate testing effort
- Create test plan for sprint
- Identify risks

---

### 2. Test Design

**Коли:** Під час розробки
**Учасники:** QA Engineers

**Активності:**
- Write test cases (manual + automated)
- Prepare test data
- Design automation scripts
- Review test cases

**Template (Test Case):**
```markdown
## TC-XXX: [Title]

**Priority:** Critical/High/Medium/Low
**Type:** Functional/Integration/E2E
**Status:** Draft/Ready/Automated

**Preconditions:**
- [list]

**Steps:**
1. [step]
   Expected: [result]
2. [step]
   Expected: [result]

**Expected Result:**
- [overall result]

**Test Data:**
- [data needed]

**Automation:** Yes/No
**Automated by:** Playwright/xUnit/etc
```

---

### 3. Test Execution

#### Cycle 1: Feature Testing
**Коли:** Feature ready на staging
**Scope:**
- Test new feature
- Related regression
- API testing

#### Cycle 2: Integration Testing
**Коли:** Multiple features integrated
**Scope:**
- Cross-feature testing
- Integration points
- E2E flows

#### Cycle 3: Regression Testing
**Коли:** Before release (weekly/bi-weekly)
**Scope:**
- Full regression suite
- Smoke tests
- Performance tests

---

### 4. Bug Reporting

**Bug Report Template:**

```markdown
# BUG-XXX: [Title]

**Priority:** Critical/High/Medium/Low
**Severity:** Blocker/Major/Minor/Trivial
**Status:** Open/In Progress/Fixed/Verified/Closed
**Environment:** Staging/Production
**Found in:** Sprint XX, Build XXX

## Description
[Clear description of the bug]

## Steps to Reproduce
1. [step]
2. [step]
3. [step]

## Expected Behavior
[what should happen]

## Actual Behavior
[what actually happens]

## Screenshots/Logs
[attach]

## Impact
[how it affects users]

## Suggested Fix (optional)
[if known]
```

**Bug Severity:**
- **Critical:** Payment fails, data loss, security breach
- **High:** Major feature unusable, affects many users
- **Medium:** Feature partially works, workaround exists
- **Low:** Minor UI issue, affects few users

**Bug Workflow:**
```
Open → Assigned → In Progress → Fixed → Ready for Testing → Verified → Closed
                                                         ↓
                                                      Reopened
```

---

### 5. Test Reporting

#### Daily Standup Update
```markdown
Yesterday:
- Tested US-101, US-102
- Found 2 bugs (BUG-201, BUG-202)
- Automated 5 test cases

Today:
- Continue US-103
- Regression test US-101 (after fix)
- Review automation PR

Blockers:
- Waiting for staging deploy
```

#### Sprint Test Summary
```markdown
## Sprint XX - QA Summary

### Test Execution
- Total test cases: 150
- Passed: 142 (95%)
- Failed: 5 (3%)
- Blocked: 3 (2%)

### Bug Statistics
- Bugs found: 18
  - Critical: 1 (fixed)
  - High: 4 (3 fixed, 1 in progress)
  - Medium: 8 (6 fixed)
  - Low: 5 (3 fixed)
- Bugs remaining: 7 (0 critical, 1 high, 6 medium/low)

### Test Coverage
- Features tested: 8/8 (100%)
- Regression: 85%
- Automation: 65%

### Risks
- 1 High bug (BUG-205) - MonoBank webhook delay
- Recommended: Monitor in production

### Recommendation
**GO for Release** ✅ (with monitoring plan)
```

---

## Чек-лист перед релізом

### 1 Тиждень до релізу

- [ ] Review release scope (features, bug fixes)
- [ ] Update test cases for new features
- [ ] Prepare test data (staging refresh)
- [ ] Schedule regression testing
- [ ] Notify stakeholders about testing schedule
- [ ] Review deployment plan

### 3 Дні до релізу

- [ ] Execute full regression suite
- [ ] Verify all Critical/High bugs fixed
- [ ] Performance testing completed
- [ ] Security scan completed (OWASP ZAP)
- [ ] Cross-browser testing
- [ ] Mobile responsive testing
- [ ] API integration tests passed
- [ ] Database migration tested (staging)

### 1 День до релізу

- [ ] Smoke test на staging (final build)
- [ ] Verify deployment scripts
- [ ] Backup verification tested
- [ ] Rollback plan documented and reviewed
- [ ] Monitoring alerts configured
- [ ] Communication plan ready (users, team)
- [ ] Post-deployment test plan ready

### День релізу

- [ ] Pre-deployment smoke test
- [ ] Deploy to production (off-peak hours)
- [ ] Post-deployment smoke test
  - [ ] Homepage loads
  - [ ] Login works
  - [ ] Create calendar works
  - [ ] Payment flow works (test mode)
  - [ ] Admin panel accessible
  - [ ] API health check
- [ ] Monitor logs for errors (30 min)
- [ ] Monitor metrics (response time, error rate)
- [ ] Verify critical integrations (MonoBank, Nova Poshta)
- [ ] Notify team: deployment successful

### Перші 24 години після релізу

- [ ] Continuous monitoring (errors, performance)
- [ ] Customer support готовність
- [ ] Quick response to critical issues
- [ ] Collect user feedback
- [ ] Daily metrics review

---

## Smoke Test Suite (Post-Deployment)

**Тривалість:** 15 минут
**Запускати:** Після кожного деплою на production

### Критичні перевірки:

1. **Infrastructure**
   - [ ] Website доступний (https://calendary.com)
   - [ ] SSL сертифікат валідний
   - [ ] API endpoint доступний (health check)
   - [ ] Database connection working

2. **Authentication**
   - [ ] Login працює
   - [ ] Logout працює
   - [ ] Session зберігається

3. **Core Features**
   - [ ] Головна сторінка відображається коректно
   - [ ] Каталог шаблонів завантажується
   - [ ] Create calendar (до кроку save)
   - [ ] Завантаження зображення працює

4. **Integrations**
   - [ ] MonoBank API доступний (create invoice test)
   - [ ] Nova Poshta API доступний (search city test)
   - [ ] Email service working (test email)

5. **Admin Panel**
   - [ ] Admin login працює
   - [ ] Dashboard loads
   - [ ] Orders list доступний

**Якщо будь-який пункт fails → ALERT → Розслідування → Rollback (якщо критично)**

---

## Метрики та KPI

### QA Metrics

#### 1. Test Coverage
**Формула:** (Features tested / Total features) × 100%
**Target:** > 95%

#### 2. Automation Coverage
**Формула:** (Automated tests / Total tests) × 100%
**Target:** > 70% (для regression suite)

#### 3. Defect Density
**Формула:** Bugs found / Features delivered
**Target:** < 3 bugs per feature

#### 4. Defect Removal Efficiency
**Формула:** (Bugs found pre-release / Total bugs) × 100%
**Target:** > 90%

#### 5. Test Execution Rate
**Формула:** Tests executed / Sprint
**Target:** Stable trend (не зменшується)

#### 6. Pass Rate
**Формула:** (Tests passed / Tests executed) × 100%
**Target:** > 90%

#### 7. Escaped Defects
**Формула:** Bugs found in production (per release)
**Target:** < 5 (< 2 for critical)

---

### Quality Gates

**Gate 1: Feature Complete**
- [ ] Unit tests > 80% coverage
- [ ] No build failures
- [ ] Code review approved

**Gate 2: QA Tested**
- [ ] Functional tests passed
- [ ] 0 Critical bugs
- [ ] High bugs < 2

**Gate 3: Ready for Release**
- [ ] Regression passed > 95%
- [ ] Performance SLA met
- [ ] Security scan clean
- [ ] 0 Critical, 0 High bugs
- [ ] Stakeholder approval

---

### Bug Metrics (Track Weekly)

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| Open Critical Bugs | 0 | > 0 |
| Open High Bugs | < 3 | > 5 |
| Average Bug Fix Time | < 2 days | > 5 days |
| Reopened Bugs % | < 5% | > 10% |
| Bugs in Production | < 5/month | > 10/month |

---

## Continuous Improvement

### Retrospective Topics (QA)

**Після кожного спринту обговорюємо:**
1. Що пройшло добре?
2. Які були проблеми/challenges?
3. Які баги пройшли через QA (escaped)?
4. Як покращити процес?
5. Які tests варто автоматизувати?

### Test Process Improvements

**Quarterly Review:**
- Аналіз escaped defects (root cause)
- Automation ROI
- Test coverage gaps
- Tool effectiveness
- Training needs

---

## Appendix

### Useful Resources

**Learning:**
- ISTQB Certification
- Test Automation University (free courses)
- "Lessons Learned in Software Testing" - Cem Kaner
- Ministry of Testing community

**Tools Documentation:**
- Playwright: https://playwright.dev/
- k6: https://k6.io/docs/
- xUnit: https://xunit.net/
- OWASP: https://owasp.org/

**Internal:**
- [Code Quality Rules](./code-quality-rules.md)
- [Architecture](./architecture.md)
- [QA Engineer Guide](./agents/qa-engineer/README.md)

---

## Контакти

**QA Team:**
- QA Tech Lead: [ім'я]
- QA Engineers: [імена]

**Collaboration:**
- Tech Lead: [ім'я]
- DevOps: [ім'я]
- Product Owner: [ім'я]

**Communication Channels:**
- Slack: #qa-team
- Daily Standup: 10:00 AM
- Bug Triage: Вівторок/Четвер 4 PM

---

**Версія:** 1.0
**Останнє оновлення:** 2025-11-15
**Наступний review:** 2025-12-15
