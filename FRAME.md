# FRAME: AI-Driven Architecture для Calendary

**Версія**: 2.0
**Дата**: 2025-11-15
**Статус**: Active

---

## ОГЛЯД

Цей документ визначає **рамку (FRAME)** AI-орієнтованої розробки проекту Calendary, де штучний інтелект виступає головним архітектором та інженером, а людина — оркестратором, що формує намір, задає обмеження та контролює відповідність результатів.

---

## ОСНОВНІ ПРИНЦИПИ

### 1. AI — головний архітектор

AI (Claude/GPT/Gemini) **повністю відповідає** за:

- **Структуру модулів** — розбиття системи на логічні компоненти
- **ERD/таблиці** — проектування бази даних, зв'язків, індексів
- **API-контракти** — endpoints, request/response models, HTTP methods
- **Статусні машини** — життєві цикли сутностей (Order, Training, Payment, тощо)
- **Інтеграційні точки** — зовнішні API, webhooks, message queues
- **Життєві цикли модулів** — створення, оновлення, видалення, архівування
- **Розширюваність** — стратегії масштабування, plugin architecture
- **Підтримку** — versioning, backward compatibility, deprecation policy

**Людина НЕ проектує архітектуру** — вона задає INTENT (намір) і CONSTRAINTS (обмеження).

---

### 2. AI — інженер реалізації

AI генерує **весь код**, включаючи:

- **Backend**: C# (.NET 9.0), ASP.NET Core API, Entity Framework, Services, Repositories
- **Frontend**: Angular 20, TypeScript, Components, Services, Guards, Interceptors
- **Database**: SQL migrations, seed data, stored procedures (якщо потрібно)
- **Tests**: Unit tests (xUnit), Integration tests, E2E tests
- **DTO/Models**: Request/Response models, DTOs, Entities, Mapping profiles
- **Infrastructure**: Docker configurations, CI/CD pipelines, deployment scripts

**Роль людини**: інтегрувати згенерований код в репозиторій, запустити, зробити локальну корекцію через GitHub Copilot.

---

### 3. Цикл розробки

```
INTENT → FRAME → AI ARCHITECTURE → AI ENGINEERING → REVIEW → RELEASE
   ↑                                                               ↓
   └───────────────────── TELEMETRY & FEEDBACK ────────────────────┘
```

#### Етапи:

1. **INTENT** — людина формує намір ("що має стати правдою")
2. **FRAME** — людина перевіряє відповідність наміру рамці (дозволено/заборонено)
3. **AI ARCHITECTURE** — AI проектує архітектурне рішення
4. **AI ENGINEERING** — AI генерує код, тести, міграції
5. **REVIEW** — людина валідує відповідність INTENT/FRAME/INVARIANTS
6. **RELEASE** — інтеграція, тестування, деплой
7. **TELEMETRY** — AI аналізує поведінку системи, пропонує покращення
8. **INTENT 2.0** — нове коло на базі зворотного зв'язку

---

### 4. Роль людини: оркестратор

Людина **НЕ** пише архітектуру, **НЕ** пише код.

Людина відповідає за:

#### 4.1 Формування INTENT (намір)
- Що має бути реалізовано?
- Яка бізнес-цінність?
- Які критерії успіху?

**Приклад INTENT**:
```
"Користувач має змогу створити календар з 12 AI-згенерованих зображень,
додати важливі дати, обрати мову та день початку тижня,
а система має згенерувати PDF та відправити на друк після оплати."
```

#### 4.2 Визначення FRAME (рамка)
- Що дозволено / заборонено?
- Які технології використовувати?
- Які обмеження ресурсів?

**Приклад FRAME**:
```
Дозволено:
- .NET 9.0, Angular 20, MS SQL Server
- Replicate API для AI генерації
- MonoBank API для оплати

Заборонено:
- PHP, Ruby, Python backend
- PostgreSQL (тільки MS SQL)
- Сторонні payment gateway окрім MonoBank
```

#### 4.3 Визначення INVARIANTS (інваріанти)
- Що не може бути порушено ніколи?
- Які критичні правила безпеки?
- Які бізнес-правила незмінні?

**Приклад INVARIANTS**:
```
- JWT token завжди має закінчуватись за 24 години
- Order не може бути видалений після оплати, тільки cancelled
- User email має бути верифікований перед створенням моделі
- Payment webhook має бути ідемпотентним
```

#### 4.4 INTENT VALIDATION (валідація наміру)
Після кожного етапу людина перевіряє:
- Чи відповідає архітектурне рішення наміру?
- Чи згенерований код реалізує задачу?
- Чи дотримані FRAME та INVARIANTS?
- Чи немає over-engineering або under-engineering?

#### 4.5 Оркестрація моделей
- Розподіл задач між різними AI (Claude, GPT, Gemini)
- Паралельне виконання незалежних задач
- Інтеграція результатів

---

### 5. Розподіл ролей між AI моделями

| AI Model | Роль | Відповідальність |
|----------|------|------------------|
| **Claude** | Концептуальний архітектор | - Архітектурні концепції<br>- Статусні машини<br>- Пояснення логіки<br>- Технічна документація<br>- Code review |
| **Codex/GPT** | Інженер коду | - Backend API (.NET)<br>- Frontend (Angular)<br>- SQL queries/migrations<br>- Unit/Integration tests<br>- UI components |
| **Gemini** | Дата-архітектор | - ERD діаграми<br>- Database schema design<br>- Data validation<br>- Analytics queries<br>- Performance optimization |
| **Copilot** | Локальний редактор | - Редагування в IDE<br>- Локальні зміни<br>- Запуск скриптів<br>- Git операції |
| **Flux/Replicate** | Візуальний генератор | - AI-генерація зображень<br>- Тренування LoRA моделей<br>- Промпт-інжиніринг |

#### Правила розподілу:

1. **Концептуальна задача** → Claude
   - "Спроектуй статусну машину для Order"
   - "Поясни архітектурне рішення для async processing"

2. **Кодогенерація** → GPT/Codex
   - "Згенеруй OrderController з CRUD endpoints"
   - "Створи Angular component для Calendar Builder"

3. **Дата-структури** → Gemini
   - "Спроектуй ERD для Orders та OrderItems"
   - "Оптимізуй запити до Users таблиці"

4. **Локальні правки** → Copilot
   - "Додай validation у форму"
   - "Виправ typo в коментарі"

5. **Візуали** → Flux
   - "Згенеруй зображення 'TOK person at the beach'"

---

### 6. Паралельність виконання

**Принцип**: Задачі нарізаються так, щоб кожен AI міг виконати їх у власному стеку без конфліктів.

#### Приклад паралельного виконання:

**Задача**: Додати функціонал "Промо-коди"

**Розподіл на паралельні задачі**:

| AI | Задача | Опис |
|----|--------|------|
| Claude | Architecture Design | Спроектувати структуру PromoCode (тип, знижка, термін дії, обмеження) |
| Gemini | Database Schema | Створити таблицю PromoCodes, зв'язки з Orders |
| GPT | Backend API | Згенерувати PromoCodeController, PromoCodeService, Repository |
| GPT | Frontend UI | Створити Angular компонент для введення промо-коду в Checkout |
| GPT | Tests | Написати unit tests для PromoCodeService |

**Людина**:
- Формує INTENT: "Користувач має змогу ввести промо-код при checkout та отримати знижку"
- Розподіляє задачі між AI
- Інтегрує результати в репозиторій
- Тестує флоу end-to-end

---

### 7. Цикл зворотного зв'язку (Feedback Loop)

```
AI ARCHITECTURE
    ↓
REVIEW: відповідність INTENT/FRAME/INVARIANTS?
    ↓
AI EXECUTION (генерація коду)
    ↓
INTEGRATION (людина інтегрує код)
    ↓
LOCAL VALIDATION (тестування флоу)
    ↓
RELEASE (деплой на production)
    ↓
TELEMETRY (AI аналізує поведінку)
    ↓
INTENT 2.0 (покращення на базі даних)
```

#### Критерії на кожному етапі:

| Етап | Критерій | Хто перевіряє |
|------|----------|---------------|
| **AI Architecture** | Відповідність INTENT, дотримання FRAME | Людина |
| **AI Execution** | Якість коду, наявність тестів | Людина + Claude (code review) |
| **Integration** | Успішна компіляція, відсутність конфліктів | Людина |
| **Local Validation** | Флоу працює end-to-end | Людина |
| **Release** | Production deployment успішний | DevOps / Людина |
| **Telemetry** | Метрики в нормі, помилок немає | AI (Gemini/Claude) |

---

### 8. UI/UX тестування AI

AI **обов'язково** перевіряє:

#### 8.1 Орієнтація користувача
- Де користувач знаходиться в системі? (breadcrumbs, active menu)
- Чи зрозуміло, що він бачить? (заголовки, підказки)
- Чи є зворотній зв'язок на дії? (spinners, success/error messages)

#### 8.2 Наступна дія
- Що користувач має зробити далі? (CTA buttons)
- Чи зрозумілі кнопки? ("Продовжити", "Зберегти", "Оплатити")
- Чи немає тупикових станів? (завжди є вихід)

#### 8.3 Зворотний зв'язок
- Чи отримує користувач підтвердження дії? (toast notifications)
- Чи показуються помилки зрозуміло? ("Неправильний email" замість "Error 400")
- Чи є loading states? (skeleton screens, spinners)

#### 8.4 Помилкові сценарії (Edge Cases)
- Що буде, якщо API не відповість?
- Що буде, якщо користувач натисне "Назад"?
- Що буде, якщо введе некоректні дані?
- Що буде, якщо сесія закінчиться?

#### 8.5 Завершеність флоу
- Чи завершується кожен флоу логічно? (success page, redirect)
- Чи може користувач повернутись до початку? (clear cart, start over)

**AI генерує**:
- UX-тест-кейси (Gherkin scenarios)
- Edge cases список
- User flow діаграми

**Приклад UX-тест-кейсу**:
```gherkin
Feature: Checkout Flow

Scenario: Successful order placement
  Given user has calendar in cart
  When user fills delivery address
  And user selects payment method
  And user clicks "Оплатити"
  Then system creates order
  And system redirects to MonoBank payment page
  And system shows success message after payment

Scenario: Payment fails
  Given user is on payment page
  When payment fails
  Then system shows error message
  And user can retry payment
  And order status is "pending_payment"
```

---

### 9. Правило: одна задача = один AI-виклик

Кожний запит до AI має бути **чітко сформульований** з одним типом виходу.

#### Типи задач:

| Тип задачі | Формат запиту | Очікуваний результат |
|------------|---------------|---------------------|
| **Code** | "Згенеруй [компонент/сервіс/контролер] для [функція]" | Готовий код з коментарями |
| **Architecture** | "Спроектуй [модуль/систему/статусну машину] для [задача]" | Діаграма/опис/рішення |
| **Explanation** | "Поясни як працює [компонент/процес]" | Текстовий опис з прикладами |
| **Refactor** | "Рефактори [код] щоб [покращення]" | Оновлений код |
| **Tests** | "Напиши тести для [компонент]" | Unit/Integration tests |
| **Data** | "Спроектуй схему для [сутність]" | SQL DDL / ERD |

#### Приклади правильних запитів:

✅ **Добре**:
```
"Згенеруй OrderController з endpoints:
- POST /api/orders (create order)
- GET /api/orders/:id (get order by id)
- PATCH /api/orders/:id/status (update status)

Включи validation, error handling, JWT authorization."
```

❌ **Погано**:
```
"Зроби щось з замовленнями"
```

✅ **Добре**:
```
"Спроектуй статусну машину для Order з станами:
pending → paid → printing → shipped → delivered → completed
та cancelled (можливий з будь-якого стану до shipped)"
```

❌ **Погано**:
```
"Поясни як працюють замовлення і згенеруй код"
```

---

### 10. Інваріанти проекту Calendary

Ці правила **не можуть бути порушені** ніколи:

#### 10.1 Безпека
- JWT token завжди має `exp` claim (24 години)
- Passwords зберігаються тільки hashed (BCrypt)
- Webhook endpoints перевіряють підпис (signature validation)
- File uploads обмежені розміром (max 10MB per image)
- HTTPS всюди (на production)

#### 10.2 Дані
- User email має бути унікальним
- Order не може бути видалений після статусу `paid`
- FluxModel.version має бути заповнений після успішного training
- Invoice має бути прив'язаний до Order (foreign key constraint)

#### 10.3 Бізнес-логіка
- Calendar має рівно 12 зображень (по одному на місяць)
- Order.total розраховується автоматично (не вводиться вручну)
- Payment webhook має бути ідемпотентним (повторний виклик не створює дублікат)
- Training не може бути перезапущений якщо status = `succeeded`

#### 10.4 API
- Всі endpoints повертають стандартні HTTP status codes
- Error responses мають формат: `{ "error": { "message": "...", "code": "..." } }`
- Success responses мають формат: `{ "data": {...} }`
- Pagination завжди має `page`, `pageSize`, `totalCount`

#### 10.5 Frontend
- Loading states показуються для всіх async операцій
- Error messages відображаються user-friendly (не "Error 500")
- Forms мають client-side validation перед відправкою
- Навігація не ламає browser history (back/forward працюють)

---

## СТРУКТУРА ДОКУМЕНТАЦІЇ

Для кожної задачі AI має створювати:

### 1. Architecture Decision Record (ADR)

```markdown
# ADR-XXX: [Title]

**Status**: Accepted / Rejected / Deprecated
**Date**: YYYY-MM-DD
**Deciders**: Claude / GPT / Gemini
**Context**: [What is the issue we're trying to solve?]

## Decision
[What did we decide?]

## Consequences
**Positive**:
- [Benefit 1]
- [Benefit 2]

**Negative**:
- [Drawback 1]

## Alternatives Considered
- [Alternative 1] — [why rejected]
- [Alternative 2] — [why rejected]
```

### 2. Implementation Plan

```markdown
# Implementation: [Feature Name]

## INTENT
[What needs to become true?]

## ARCHITECTURE
[How will it be structured?]

## TASKS
- [ ] Task 1 (AI: Claude)
- [ ] Task 2 (AI: GPT)
- [ ] Task 3 (AI: Gemini)

## VALIDATION
- [ ] Validation criterion 1
- [ ] Validation criterion 2

## ROLLBACK PLAN
[What to do if something goes wrong?]
```

### 3. API Contract

```markdown
# API Contract: [Endpoint Name]

**Method**: POST / GET / PUT / PATCH / DELETE
**Path**: `/api/...`
**Auth**: JWT Required / Public

## Request
```json
{
  "field": "type"
}
```

## Response (Success)
```json
{
  "data": { ... }
}
```

## Response (Error)
```json
{
  "error": {
    "message": "...",
    "code": "..."
  }
}
```

## Validation Rules
- [Rule 1]
- [Rule 2]
```

---

## КРИТЕРІЇ ПРИЙНЯТТЯ РІШЕНЬ

### Коли використовувати Claude?
- Концептуальні питання архітектури
- Пояснення складної логіки
- Code review
- Документація (ADR, Implementation Plans)

### Коли використовувати GPT/Codex?
- Генерація коду (C#, TypeScript, SQL)
- Unit/Integration tests
- Рефакторинг існуючого коду
- API endpoints

### Коли використовувати Gemini?
- Проектування бази даних (ERD)
- Оптимізація запитів
- Аналітика даних
- Performance tuning

### Коли використовувати Copilot?
- Локальні правки в IDE
- Швидкі зміни в існуючих файлах
- Git операції
- Запуск команд

### Коли використовувати Flux/Replicate?
- Генерація зображень для календарів
- Тренування LoRA моделей
- Експерименти з промптами

---

## WORKFLOW: Типовий цикл задачі

### Приклад: Додати функціонал "Історія замовлень користувача"

#### 1. INTENT (людина)
```
"Користувач має змогу переглянути всі свої замовлення з фільтрацією по статусу,
сортуванням по даті, та пагінацією."
```

#### 2. FRAME CHECK (людина)
```
✅ Дозволено: .NET API, Angular, MS SQL
✅ Використовуємо JWT для авторизації
✅ Pagination стандартна (page, pageSize)
```

#### 3. AI ARCHITECTURE (Claude)
**Запит**:
```
"Спроектуй архітектуру для функції 'Історія замовлень користувача'
з фільтрацією по статусу, сортуванням, пагінацією."
```

**Результат**:
- ADR документ
- API contract
- Database query plan
- Frontend components structure

#### 4. TASKS DISTRIBUTION (людина)

| AI | Задача |
|----|--------|
| Gemini | Оптимізувати SQL query для Orders з JOIN на OrderItems |
| GPT | Згенерувати OrdersController з GET /api/orders endpoint |
| GPT | Згенерувати Angular OrderHistoryComponent з таблицею та фільтрами |
| GPT | Написати tests для OrdersController |

#### 5. AI EXECUTION (паралельно)
Кожен AI генерує свою частину незалежно.

#### 6. INTEGRATION (людина)
- Інтегрує згенерований код в репозиторій
- Запускає `dotnet build`, `ng build`
- Виправляє конфлікти (якщо є)

#### 7. LOCAL VALIDATION (людина)
- Тестує флоу: Login → Profile → Order History
- Перевіряє фільтрацію, сортування, пагінацію
- Тестує edge cases (порожній список, 100+ замовлень)

#### 8. CODE REVIEW (Claude)
**Запит**:
```
"Зроби code review для OrdersController та OrderHistoryComponent.
Перевір:
- дотримання C#/TypeScript best practices
- error handling
- security (authorization)
- performance (N+1 problem)"
```

#### 9. RELEASE
- Commit з описовим message
- Push на feature branch
- Pull Request з описом змін
- Merge після approval
- Deploy на production

#### 10. TELEMETRY (AI)
**Запит до Gemini**:
```
"Проаналізуй логи за тиждень для endpoint GET /api/orders.
Які найчастіші помилки? Які фільтри найпопулярніші?"
```

**Результат**:
- Звіт з analytics
- Пропозиції покращень (caching, indexing)

#### 11. INTENT 2.0
На базі telemetry:
```
"Додати кешування для Order History, щоб зменшити навантаження на БД"
```

Цикл повторюється.

---

## FORBIDDEN PRACTICES (Заборонено)

Ці практики **суворо заборонені**:

### ❌ Людина пише архітектуру вручну
**Замість**: Формулює INTENT і делегує AI

### ❌ Людина пише код вручну (окрім локальних правок)
**Замість**: AI генерує код, людина інтегрує

### ❌ Запит до AI без чіткого типу задачі
**Замість**: Використовувати формат "code/architecture/tests/..."

### ❌ Виконання залежних задач паралельно
**Замість**: Розбити на незалежні або виконувати послідовно

### ❌ Прийняття архітектурних рішень без ADR
**Замість**: Завжди документувати через ADR

### ❌ Деплой без Local Validation
**Замість**: Завжди тестувати локально перед release

### ❌ Ігнорування INVARIANTS
**Замість**: Перевіряти відповідність інваріантам на code review

---

## METRICS & KPI

Для оцінки ефективності AI-Driven підходу:

### Швидкість розробки
- **Time to MVP** — скільки часу від INTENT до working feature
- **AI Response Time** — середній час генерації коду
- **Integration Time** — скільки часу людина витрачає на інтеграцію

### Якість коду
- **Test Coverage** — % коду покритого тестами (мета: >80%)
- **Code Review Issues** — кількість issues на code review
- **Production Bugs** — кількість багів після release

### Відповідність FRAME
- **FRAME Violations** — скільки разів порушена рамка
- **INVARIANTS Violations** — скільки разів порушені інваріанти

### Ефективність моделей
- **Claude Usage** — кількість architecture tasks
- **GPT Usage** — кількість code generation tasks
- **Gemini Usage** — кількість data/analytics tasks
- **Success Rate** — % задач виконаних з першого разу

---

## EVOLUTION STRATEGY

FRAME є **живим документом** і має еволюціонувати.

### Коли оновлювати FRAME?

1. **Нова технологія додана** (наприклад, міграція з MS SQL на PostgreSQL)
2. **Новий INVARIANT** (нове бізнес-правило, яке не може бути порушено)
3. **Новий AI доданий** (наприклад, використання Claude Opus для складніших задач)
4. **Процес змінився** (новий етап у циклі розробки)
5. **Метрики показують проблему** (багато FRAME violations → треба уточнити правила)

### Версіонування FRAME

```
FRAME 1.0 — Initial version
FRAME 2.0 — Added AI-Driven Architecture principles
FRAME 2.1 — Added Gemini for data tasks
FRAME 3.0 — Migration to microservices (future)
```

---

## ПРИКЛАДИ З ПРАКТИКИ

### Приклад 1: Додавання промо-кодів

**INTENT**:
```
Користувач має змогу ввести промо-код при checkout та отримати знижку.
Промо-коди мають термін дії, тип знижки (%, fixed), обмеження по кількості використань.
```

**FRAME CHECK**:
```
✅ .NET API, Angular frontend
✅ MS SQL database
✅ Промо-код валідується на backend (не frontend)
```

**INVARIANTS**:
```
- Промо-код не може бути застосований двічі до одного Order
- Знижка не може перевищувати Order.total (мінімум 0)
- Expired промо-коди не валідні
```

**ARCHITECTURE (Claude)**:
```markdown
# ADR-015: Promo Codes System

## Decision
Створити таблицю PromoCodes:
- code (unique)
- discount_type (percentage/fixed)
- discount_value
- expires_at
- max_uses
- current_uses

Order має поле promo_code_id (nullable FK)
```

**TASKS**:
- Gemini: Design PromoCodes table schema
- GPT: Generate PromoCodeController, PromoCodeService
- GPT: Generate Angular promo-code-input component
- GPT: Write tests for promo code validation logic

**VALIDATION**:
- [ ] Користувач може ввести промо-код в checkout
- [ ] Система показує розмір знижки
- [ ] Expired промо-код відхиляється
- [ ] Промо-код з max_uses = current_uses відхиляється

---

### Приклад 2: Оптимізація Order History

**INTENT**:
```
Order History завантажується повільно для користувачів з 50+ замовленнями.
Треба оптимізувати query та додати caching.
```

**FRAME CHECK**:
```
✅ Використовуємо Redis для кешування (вже є в infrastructure)
✅ Cache invalidation при зміні Order
```

**INVARIANTS**:
```
- Cache не може бути stale більше 5 хвилин
- User бачить актуальні дані одразу після зміни Order
```

**ARCHITECTURE (Claude)**:
```markdown
# ADR-016: Order History Caching

## Decision
- Cache Orders для кожного User в Redis (key: `orders:user:{userId}`)
- TTL = 5 minutes
- Invalidate cache on Order update/create
```

**TASKS**:
- Gemini: Analyze slow query, suggest indexes
- GPT: Implement Redis caching in OrderService
- GPT: Add cache invalidation logic
- GPT: Update OrdersController to use cached data

**VALIDATION**:
- [ ] Order History завантажується < 500ms
- [ ] Cache invalidates при зміні Order
- [ ] Pagination працює з cached data

---

## CONCLUSION

**FRAME** — це контракт між людиною та AI:

- **AI** відповідає за архітектуру та код
- **Людина** відповідає за намір, рамки, оркестрацію

Дотримання цього підходу забезпечує:
- **Швидкість** — AI генерує код набагато швидше людини
- **Якість** — AI не робить typo, дотримується best practices
- **Consistency** — всі компоненти мають однаковий стиль
- **Scalability** — паралельне виконання задач різними AI
- **Flexibility** — легко змінити INTENT і перегенерувати

**Наступний крок**: Взяти задачу з TASKS.md, сформулювати INTENT, і почати цикл.

---

**Останнє оновлення**: 2025-11-15
**Автор**: AI + Human Orchestrator
**Версія**: 2.0
