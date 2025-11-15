# Calendary - Документація для AI помічників

## Опис проекту

**Calendary.com.ua** - онлайн-сервіс для створення персоналізованих календарів-постерів з використанням AI генерації зображень.

### Основний функціонал:
- Реєстрація та авторизація користувачів (JWT)
- Завантаження фото користувача для створення AI моделі
- Тренування LoRA моделі через Replicate API (Flux)
- Генерація персоналізованих зображень для кожного місяця
- Створення календаря з вибором мови, важливих дат
- Генерація PDF файлу календаря
- Оформлення замовлення з інтеграцією Нова Пошта та MonoPay
- Адмін-панель для керування замовленнями

## Технологічний стек

### Backend (.NET 9.0)
- **Calendary.Api** - ASP.NET Core Web API
  - Controllers для роботи з користувачами, замовленнями, AI генерацією
  - JWT авторизація
  - Інтеграція з Replicate API
  - Генерація PDF календарів
  - WebHook endpoints для Replicate та MonoPay

- **Calendary.Consumer** - RabbitMQ consumer
  - Обробка асинхронних задач
  - Генерація зображень та календарів у фоні

- **Calendary.Core** - Business Logic Layer
  - Services: ReplicateService, PdfService, EmailService, SmsService
  - Providers: NovaPoshtaProvider, MonoPayProvider
  - Helpers та утиліти

- **Calendary.Repos** - Data Access Layer
  - Entity Framework Core
  - MS SQL Server
  - Repository pattern
  - Migrations

- **Calendary.Model** - Shared Models
  - DTOs
  - Messages для RabbitMQ
  - Entities

### Frontend (Angular 18)
- **Calendary.Ng** - Angular SPA + SSR
  - Material Design UI
  - Сторінки: Landing, Registration, Calendar Builder, Cart, Checkout, Profile
  - Адмін-панель: управління замовленнями, моделями, користувачами
  - Guards для авторизації
  - Interceptors для HTTP запитів (JWT token)

### Зовнішні інтеграції
1. **Replicate API** - генерація AI зображень
   - Створення та тренування LoRA моделей (Flux)
   - Генерація зображень на основі промптів
   - WebHook для отримання результатів

2. **Нова Пошта API** - доставка
   - Отримання списку міст, відділень
   - Розрахунок вартості доставки

3. **MonoPay API** - оплата
   - Створення інвойсів
   - WebHook для підтвердження оплати

4. **RabbitMQ** - черга повідомлень
   - Асинхронна обробка задач (генерація зображень, PDF)

5. **SMTP/SMS** - нотифікації
   - Email верифікація
   - SMS верифікація
   - Повідомлення про статус замовлення

## Структура проекту

```
Calendary/
├── src/
│   ├── Calendary.Api/          # ASP.NET Core API
│   │   ├── Controllers/        # API endpoints
│   │   ├── Dtos/              # Data Transfer Objects
│   │   ├── Profiles/          # AutoMapper profiles
│   │   ├── Providers/         # External API providers
│   │   ├── Tools/             # Utilities
│   │   └── wwwroot/           # Static files
│   │
│   ├── Calendary.Consumer/     # RabbitMQ consumer
│   │   └── Consumers/         # Message handlers
│   │
│   ├── Calendary.Core/         # Business logic
│   │   ├── Helpers/           # Helper classes
│   │   ├── Models/            # Domain models
│   │   ├── Providers/         # External integrations
│   │   ├── Senders/           # Email/SMS senders
│   │   ├── Services/          # Business services
│   │   ├── Settings/          # Configuration models
│   │   └── Templates/         # Email/PDF templates
│   │
│   ├── Calendary.Model/        # Shared models
│   │   └── Messages/          # RabbitMQ messages
│   │
│   ├── Calendary.Repos/        # Data access
│   │   ├── Migrations/        # EF migrations
│   │   └── Repositories/      # Repository implementations
│   │
│   └── Calendary.Ng/           # Angular frontend
│       └── src/
│           ├── app/
│           │   ├── admin/     # Admin panel
│           │   ├── components/# Reusable components
│           │   ├── pages/     # Page components
│           │   └── strategies/# Auth strategies
│           ├── guards/        # Route guards
│           ├── interceptors/  # HTTP interceptors
│           ├── models/        # TypeScript models
│           └── services/      # Angular services
│
├── tests/                      # Unit and integration tests
├── docs/                       # Documentation
│   ├── process.md             # Replicate API workflow
│   └── Calendary ERD.puml     # Database diagram
│
├── docker-compose.yml          # Docker setup
└── Calendary.sln              # Solution file
```

## Основні процеси

### 1. Робота з AI моделями (Replicate API)

#### Створення моделі
1. Користувач завантажує 10-20 фото
2. Backend створює архів (ZIP)
3. Викликається Replicate API для створення моделі: `POST /v1/models`
4. Запускається тренування: `POST /v1/models/{trainer}/trainings`
5. Replicate надсилає WebHook після завершення тренування
6. Зберігається `version` моделі для подальшої генерації

#### Генерація зображень
1. Формується промпт з trigger word "TOK" (наприклад: "a photo of TOK person at the beach")
2. Викликається: `POST /v1/predictions` з параметрами (seed, lora_scale, guidance_scale)
3. Replicate повертає URL згенерованого зображення
4. Зображення завантажується та зберігається локально

Детальніше: [docs/process.md](./docs/process.md)

### 2. Створення календаря

1. Користувач обирає 12 зображень (по одному на місяць)
2. Додає важливі дати
3. Вибирає мову та день початку тижня
4. Backend генерує PDF з календарем
5. Користувач додає в кошик
6. Оформлення замовлення

### 3. Оформлення замовлення

1. Користувач вводить адресу доставки (Нова Пошта)
2. Обирає спосіб оплати (MonoPay)
3. Створюється Order в БД
4. Генерується інвойс в MonoPay
5. Після оплати - WebHook від MonoPay
6. Адмін відправляє PDF на друк
7. Оновлення статусу замовлення

## База даних

Основні таблиці:
- **Users** - користувачі (email, password hash, role)
- **FluxModels** - AI моделі користувачів (replicate_id, version, status)
- **Trainings** - історія тренувань (model_id, status, completed_at)
- **PromptThemes** - теми промптів для генерації (category, season)
- **Synthesis** - згенеровані зображення (training_id, prompt, seed, image_url)
- **Calendars** - календарі користувачів (images, dates, language, start_day)
- **Orders** - замовлення (user_id, calendar_id, status, total)
- **OrderItems** - позиції замовлення
- **Invoices** - інвойси MonoPay

## Як запустити проект

### Backend
```bash
cd src/Calendary.Api
dotnet restore
dotnet run
```

### Frontend
```bash
cd src/Calendary.Ng
npm install
npm start
```

### Docker
```bash
docker-compose up -d
```

## Налаштування (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=calendary;..."
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "calendary.com.ua",
    "ExpiryMinutes": 1440
  },
  "ReplicateSettings": {
    "ApiKey": "r8_xxx...",
    "Owner": "chernikov",
    "TrainerModel": "ostris/flux-dev-lora-trainer",
    "WebhookUrl": "https://calendary.com.ua/api/webhook"
  },
  "RabbitMqSettings": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "NovaPoshtaSettings": {
    "ApiKey": "..."
  },
  "MonoPaySettings": {
    "Token": "...",
    "WebhookUrl": "..."
  }
}
```

## Тестування після змін

### Backend API
1. **Авторизація**
   - [ ] Реєстрація нового користувача
   - [ ] Login та отримання JWT token
   - [ ] Refresh token

2. **AI Моделі**
   - [ ] Завантаження фото в архів
   - [ ] Створення моделі в Replicate
   - [ ] Перевірка статусу тренування
   - [ ] Генерація тестового зображення

3. **Календарі**
   - [ ] Створення календаря з 12 зображеннями
   - [ ] Додавання важливих дат
   - [ ] Генерація PDF

4. **Замовлення**
   - [ ] Додавання в кошик
   - [ ] Розрахунок вартості доставки (Нова Пошта)
   - [ ] Створення інвойсу (MonoPay)
   - [ ] Обробка WebHook оплати

5. **Адмін-панель**
   - [ ] Перегляд замовлень
   - [ ] Зміна статусу замовлення
   - [ ] Відправка PDF на друк

### Frontend
1. **Landing page** - відображення, навігація
2. **Реєстрація/Login** - форми, валідація, помилки
3. **Calendar Builder** - вибір зображень, дати, мова
4. **Кошик** - додавання/видалення, розрахунок суми
5. **Checkout** - форма доставки, оплата
6. **Profile** - перегляд замовлень, історія
7. **Admin** - робота з замовленнями, моделями

### Інтеграції
1. **Replicate API** - перевірити WebHook після тренування/генерації
2. **Нова Пошта** - отримання міст та відділень
3. **MonoPay** - створення інвойсу, WebHook оплати
4. **RabbitMQ** - обробка повідомлень у Consumer

## Поточні технічні задачі

### Високий пріоритет
1. **Оптимізація генерації PDF** - повільна генерація для великих календарів
2. **Кешування Replicate результатів** - уникнення дублювання запитів
3. **Обробка помилок Replicate API** - retry logic для failed trainings
4. **Валідація завантажених фото** - перевірка розміру, формату, NSFW контенту

### Середній пріоритет
5. **Unit тести для Core сервісів** - покриття тестами ReplicateService, PdfService
6. **Логування всіх API запитів** - Serilog з структурованими логами
7. **Rate limiting для API** - захист від зловживань
8. **Оптимізація запитів до БД** - N+1 problem, eager loading

### Низький пріоритет
9. **Міграція на .NET 10** (після релізу)
10. **Додавання нових промпт тем** - сезонні, святкові
11. **Multilanguage підтримка UI** - українська, англійська
12. **Mobile app** - React Native або Flutter

## Корисні команди

### Git
```bash
git checkout -b feature/task-name
git add .
git commit -m "feat: add feature description"
git push origin feature/task-name
```

### .NET
```bash
dotnet ef migrations add MigrationName --project src/Calendary.Repos
dotnet ef database update --project src/Calendary.Api
dotnet build
dotnet test
```

### Angular
```bash
ng generate component components/component-name
ng generate service services/service-name
ng build --configuration production
```

### Docker
```bash
docker-compose up -d          # Запуск всіх сервісів
docker-compose logs -f api    # Логи API
docker-compose down           # Зупинка
```

## Troubleshooting

### Replicate API помилки
- **429 Too Many Requests** - rate limit, треба зачекати
- **Training failed** - перевірити якість/кількість фото в архіві
- **Invalid version** - модель ще тренується або видалена

### База даних
- **Connection timeout** - перевірити ConnectionString
- **Migration conflict** - `dotnet ef database drop` (ТІЛЬКИ на dev!)

### RabbitMQ
- **Connection refused** - перевірити що RabbitMQ запущений
- **Queue not found** - створити queue в RabbitMQ Management UI

## Контакти та ресурси

- **Replicate Docs**: https://replicate.com/docs
- **Flux LoRA Trainer**: https://replicate.com/ostris/flux-dev-lora-trainer
- **Нова Пошта API**: https://developers.novaposhta.ua/
- **MonoPay API**: https://api.monobank.ua/docs/

## Примітки для AI помічників

### Стиль коду
- **Backend**: C# 12, async/await, LINQ, dependency injection
- **Frontend**: TypeScript, RxJS, Angular best practices
- **Naming**: PascalCase (C#), camelCase (TypeScript)

### Перед початком роботи
1. Прочитайте актуальну документацію в `docs/`
2. Перевірте існуючі TODO/FIXME коментарі в коді
3. Подивіться на останні commits для розуміння контексту
4. При зміні API - оновіть відповідні Angular services

### Правила роботи
- Завжди створюйте окрему бренчу для задачі
- Пишіть описові commit messages
- Додавайте коментарі для складної логіки
- Оновлюйте цей документ при змінах архітектури
- Тестуйте зміни локально перед commit

---

**Останнє оновлення**: 2025-11-15
**Версія проекту**: 1.0
**Статус**: Production Ready
