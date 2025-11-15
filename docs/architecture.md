# Архітектура Calendary

## Огляд проєкту

Calendary - платформа для створення, редагування, замовлення та продажу персоналізованих календарів з інтеграцією систем оплати, доставки та маркетингу.

## Бізнес-модель

### Цільові сегменти
1. **B2C** - індивідуальні замовлення календарів
2. **B2B** - корпоративні замовлення (великі обсяги)

### Варіанти виконання замовлень
1. Друк через локальні друкарні + доставка Новою Поштою по Україні
2. Цифровий продукт - завантаження PDF для самостійного друку

---

## Системна архітектура

### 1. Frontend Layer

#### 1.1 Публічна частина (Customer Portal)
- **Редактор календарів**
  - Інтерфейс для створення/редагування календарів
  - Вибір шаблонів, завантаження зображень
  - Попередній перегляд
  - Підтримка різних форматів (настінні, настільні, планери)

- **Система замовлень**
  - Кошик покупок
  - Форма оформлення замовлення
  - Вибір способу доставки / цифрового продукту
  - Калькулятор вартості

- **Профіль користувача**
  - Історія замовлень
  - Збережені дизайни
  - Персональні дані

#### 1.2 Адміністративна частина

##### Адмінка замовлень (Operations Admin)
- Перегляд всіх замовлень
- Напівавтоматична обробка:
  - Підтвердження оплати
  - Відправка на друк
  - Створення ТТН Нової Пошти
  - Відстеження статусу
- Управління корпоративними замовленнями
- Аналітика продажів

##### Адмінка маркетолога (Marketing Admin)
- Управління рекламними кампаніями:
  - Instagram
  - Facebook
  - Threads
  - Google AdSense (опціонально)
- Аналітика ефективності реклами
- A/B тестування
- Управління промо-кодами та знижками
- Контент-календар
- Статистика конверсій

### 2. Backend Layer

#### 2.1 API Gateway
- Єдина точка входу для всіх клієнтських запитів
- Маршрутизація до відповідних мікросервісів
- Автентифікація та авторизація
- Rate limiting

#### 2.2 Core Services

##### Calendar Editor Service
- Обробка та збереження дизайнів календарів
- Генерація PDF для друку
- Управління шаблонами
- Обробка зображень (resize, optimization)
- Валідація макетів

##### Order Management Service
- Створення та управління замовленнями
- Обробка статусів замовлень
- Інтеграція з друкарнями
- Логіка корпоративних замовлень (bulk orders)
- Управління чергами друку

##### Payment Service
- Інтеграція з Monobank API
- Обробка платежів
- Підтвердження оплат
- Повернення коштів
- Webhook обробка

##### Delivery Service
- Інтеграція з API Нової Пошти
- Створення ТТН
- Відстеження посилок
- Розрахунок вартості доставки
- Вибір відділень

##### User Management Service
- Реєстрація та автентифікація
- Управління профілями
- Ролі та дозволи (customer, admin, marketer, accountant)
- JWT токени

##### Marketing Service
- Інтеграція з Meta Business API (Instagram, Facebook, Threads)
- Інтеграція з Google Ads API
- Управління кампаніями
- Аналітика та звітність
- Промо-коди та знижки
- Email/SMS маркетинг

##### Accounting Service
- Облік фінансових операцій
- Формування звітів
- Податкова звітність
- Інтеграція з бухгалтерськими системами
- Управління витратами (друк, доставка)
- ROI аналіз

##### Notification Service
- Email сповіщення (підтвердження замовлення, статус)
- SMS сповіщення
- Push notifications
- Webhook для зовнішніх систем

### 3. Data Layer

#### 3.1 Primary Database (PostgreSQL)
- Користувачі та автентифікація
- Замовлення
- Календарі та шаблони
- Фінансові транзакції
- Метадані файлів

#### 3.2 File Storage (S3-compatible)
- Зображення користувачів
- Шаблони календарів
- Згенеровані PDF
- Backup дизайнів

#### 3.3 Cache Layer (Redis)
- Сесії користувачів
- Кешування часто запитуваних даних
- Queue для асинхронних задач
- Rate limiting

#### 3.4 Analytics Database (ClickHouse/PostgreSQL)
- Метрики маркетингу
- Аналітика поведінки користувачів
- Звітність для бізнесу

### 4. Integration Layer

#### 4.1 Зовнішні інтеграції
- **Monobank API** - платежі
- **Nova Poshta API** - доставка
- **Meta Business Suite** - соціальні мережі
- **Google Ads API** - реклама
- **Printing Partner APIs** - локальні друкарні

#### 4.2 Внутрішні черги
- RabbitMQ / Kafka для асинхронної обробки:
  - Генерація PDF
  - Відправка email/SMS
  - Обробка зображень
  - Синхронізація з друкарнями

### 5. DevOps & Infrastructure

#### 5.1 Containerization
- Docker для всіх сервісів
- Docker Compose для локальної розробки
- Kubernetes для production

#### 5.2 CI/CD Pipeline
- GitHub Actions
- Automated testing
- Automated deployment
- Rollback механізми

#### 5.3 Monitoring & Logging
- Prometheus + Grafana для метрик
- ELK Stack для логів
- Sentry для error tracking
- Uptime monitoring

#### 5.4 Security
- HTTPS всюди
- Rate limiting
- DDoS protection
- Регулярні security audits
- Backup стратегія
- GDPR compliance

---

## Технологічний стек (поточний)

### Frontend
**Admin Panel (Angular):**
- **Framework**: Angular 18+
- **UI Library**: Angular Material / PrimeNG
- **State Management**: NgRx / Services
- **Forms**: Reactive Forms
- **HTTP Client**: HttpClient

**Customer Portal (опціонально React):**
- **Framework**: React / Next.js (для максимальної гнучкості UI/UX)
- **UI Library**: Tailwind CSS + shadcn/ui або Material-UI
- **State Management**: Zustand / Redux Toolkit
- **Canvas Editor**: Fabric.js / Konva.js (для редактора календарів)
- **Forms**: React Hook Form + Zod

### Backend
- **Runtime**: .NET 9.0
- **Framework**: ASP.NET Core Web API
- **Architecture**: Clean Architecture / Onion Architecture
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT (JSON Web Tokens)
- **Mapping**: AutoMapper
- **API Style**: REST
- **Message Queue**: RabbitMQ
- **PDF Generation**: iTextSharp / QuestPDF

**Проєкти:**
- `Calendary.Api` - Web API
- `Calendary.Core` - Business Logic
- `Calendary.Repos` - Data Access Layer (EF Core)
- `Calendary.Model` - Domain Models
- `Calendary.Consumer` - RabbitMQ Consumer
- `Calendary.Ng` - Angular Admin Panel

### Database
- **Primary**: Microsoft SQL Server (MS SQL)
- **ORM**: Entity Framework Core
- **Migrations**: EF Core Migrations
- **Cache**: Redis (опціонально для sessions та caching)

### Infrastructure
- **Container**: Docker + Docker Compose
- **CI/CD**: GitHub Actions
- **Cloud**:
  - **Поточна**: DigitalOcean Droplets
  - **Майбутня**: Azure (міграція заплановано)
- **Web Server**: Nginx (для Angular додатку)
- **Monitoring**: Azure Monitor (після міграції)
- **Logging**: Serilog / NLog

### Інтеграції (вже підключені)
- **NovaPost API** - доставка Нова Пошта
- **MonoBank API** - платежі
- **SendGrid** - email розсилки
- **SmsClub** - SMS повідомлення
- **Replicate AI** - генерація зображень/контенту (AI)

---

## Поточний стан системи

### Що вже реалізовано:
✅ Backend API (.NET 9.0)
✅ MS SQL Database з Entity Framework
✅ Angular Admin Panel
✅ Docker containerization
✅ RabbitMQ integration
✅ Інтеграції: MonoBank, NovaPost, SendGrid, SmsClub, Replicate AI
✅ JWT Authentication
✅ Deployment на DigitalOcean

### Що потрібно доопрацювати/додати:

**Frontend (Customer Portal):**
- [ ] Редактор календарів з Canvas
- [ ] Публічний каталог шаблонів
- [ ] Кошик та checkout процес
- [ ] Особистий кабінет користувача
- [ ] Можливість завантаження PDF

**Backend:**
- [ ] Розширити API для нових функцій
- [ ] Покращити обробку платежів (webhooks MonoBank)
- [ ] Корпоративні замовлення (bulk orders)
- [ ] Генерація PDF календарів
- [ ] Система промо-кодів
- [ ] Email/SMS notifications workflow

**Admin Panel:**
- [ ] Маркетингова адмінка (Meta Business Suite, Google Ads)
- [ ] Бухгалтерська адмінка (звіти, аналітика)
- [ ] Управління шаблонами календарів
- [ ] Дашборд з аналітикою

**Infrastructure:**
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Monitoring та Logging
- [ ] Backup стратегія
- [ ] Міграція на Azure

---

## Поетапний план розробки (restart roadmap)

### Phase 1: Перезапуск MVP (2-3 місяці)
**Мета: Запустити базову версію з можливістю продажу календарів**

**Frontend (Customer Portal - React):**
1. Редактор календарів
   - Вибір шаблонів
   - Завантаження фото
   - Додавання тексту
   - Попередній перегляд
2. Система замовлень
   - Кошик
   - Checkout форма
   - Інтеграція з оплатою MonoBank
3. Особистий кабінет
   - Історія замовлень
   - Збережені дизайни

**Backend:**
1. API для редактора
2. Генерація PDF
3. Webhook MonoBank
4. Створення ТТН Нова Пошта
5. Email notifications

**Admin:**
1. Базова обробка замовлень
2. Управління статусами
3. Простий дашборд

### Phase 2: Розширення функціоналу (1-2 місяці)
1. Корпоративні замовлення
2. Система промо-кодів
3. Покращена аналітика
4. Автоматизація створення ТТН

### Phase 3: Маркетинг (1-2 місяці)
1. Маркетингова адмінка
2. Інтеграція Meta Business API
3. Google Ads інтеграція
4. Email маркетинг кампанії

### Phase 4: Міграція на Azure (1 місяць)
1. Налаштування Azure infrastructure
2. Міграція бази даних
3. Переналаштування CI/CD
4. Тестування та запуск

---

## API Endpoints (приклад структури)

### Public API
```
POST   /api/auth/register
POST   /api/auth/login
GET    /api/calendars/templates
POST   /api/calendars
PUT    /api/calendars/:id
GET    /api/calendars/:id
POST   /api/orders
GET    /api/orders/:id
POST   /api/payments/create
GET    /api/delivery/calculate
```

### Admin API
```
GET    /api/admin/orders
PATCH  /api/admin/orders/:id/status
GET    /api/admin/analytics
POST   /api/admin/marketing/campaigns
GET    /api/admin/marketing/stats
GET    /api/admin/accounting/reports
```

---

## Безпека та відповідність

1. **GDPR Compliance** - захист персональних даних користувачів
2. **PCI DSS** - безпечна обробка платежів через Monobank
3. **Ukrainian Law** - відповідність законодавству України
4. **Data Backup** - щоденні резервні копії
5. **Disaster Recovery** - план відновлення після збоїв

---

## Масштабування

### Horizontal Scaling
- Мікросервісна архітектура дозволяє масштабувати окремі компоненти
- Load balancing між інстансами
- Database replication

### Vertical Scaling
- Можливість збільшення ресурсів для окремих сервісів
- Оптимізація запитів до БД

---

## Моніторинг ключових метрик

### Бізнес метрики
- Кількість замовлень
- Середній чек
- Конверсія відвідувачів → покупці
- CAC (Customer Acquisition Cost)
- LTV (Lifetime Value)
- ROI рекламних кампаній

### Технічні метрики
- Uptime системи
- Час відгуку API
- Помилки та їх частота
- Використання ресурсів
- Час генерації PDF
