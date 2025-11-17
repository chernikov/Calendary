# Backlog: GitHub Issues

## GH-136: Balance money system

**Джерело**: TASKS.md / GitHub #136
**Пріоритет**: P0 (Критичний для монетизації)
**Складність**: Дуже висока
**Час**: 15-20 годин
**AI**: ChatGPT o1 (планування) → Claude Code (виконання)

### Що треба зробити
1. Wallet система для Users
2. Операції: поповнення, списання, історія
3. Інтеграція MonoPay
4. Списання при FluxModel, генерації, темах
5. Admin panel для балансів

### Файли
- `src/Calendary.Repos/Entities/Wallet.cs`
- `src/Calendary.Repos/Entities/Transaction.cs`
- `src/Calendary.Core/Services/WalletService.cs`
- `src/Calendary.Api/Controllers/WalletController.cs`
- `src/Calendary.Ng/src/app/pages/wallet/`

---

## GH-134: Payment for theme

**Джерело**: TASKS.md / GitHub #134
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**AI**: Claude Code

**Залежність**: GH-136 (Wallet system)

### Що треба зробити
1. PromptThemes платні (окрім базових)
2. Перевірка балансу
3. Списання з wallet
4. UserTheme entity (куплені теми)

### Файли
- `src/Calendary.Repos/Entities/PromptTheme.cs`
- `src/Calendary.Repos/Entities/UserTheme.cs`
- `src/Calendary.Core/Services/ThemeService.cs`

---

## GH-133: Payment for flux model

**Джерело**: TASKS.md / GitHub #133
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**AI**: Claude Code

**Залежність**: GH-136 (Wallet system)

### Що треба зробити
1. FluxModel створення платне
2. Перевірка балансу перед training
3. Списання після успішного training
4. Refund якщо failed

### Файли
- `src/Calendary.Core/Services/FluxModelService.cs`
- `src/Calendary.Consumer/Consumers/TrainingConsumer.cs`

---

## GH-131: Many models (користувач може мати багато моделей)

**Джерело**: TASKS.md / GitHub #131
**Пріоритет**: P1 (Високий)
**Складність**: Висока
**Час**: 6-8 годин
**AI**: Gemini (аналіз) → Claude Code

**Примітка**: Частково покрито в Epic 01 (tasks 9-11), але треба додати:
- Архівування старих моделей
- Ліміти по тарифах (Free: 1, Premium: 5, Pro: unlimited)

### Додатково до Epic 01
1. IsArchived поле
2. Тарифні ліміти
3. Архівування UI

### Файли
- `src/Calendary.Repos/Entities/FluxModel.cs`
- `src/Calendary.Core/Services/FluxModelService.cs`

---

## GH-130: Image storage (Cloud)

**Джерело**: TASKS.md / GitHub #130
**Пріоритет**: P1 (Високий)
**Складність**: Висока
**Час**: 8-10 годин
**AI**: ChatGPT o1 (планування) → Claude Code

### Що треба зробити
1. Перенести з `wwwroot/uploads` на Azure Blob/AWS S3
2. Міграція існуючих файлів
3. CDN integration
4. Update всіх URLs

### Файли
- `src/Calendary.Core/Services/FileStorageService.cs` (новий)
- `src/Calendary.Core/Services/ReplicateService.cs`
- `appsettings.json` (credentials)

---

## GH-109: NgRx в Angular проекті

**Джерело**: TASKS.md / GitHub #109
**Пріоритет**: P2 (Середній)
**Складність**: Дуже висока
**Час**: 20-30 годин
**AI**: Gemini (аналіз) → Claude Code (поетапно)

### Що треба зробити
1. NgRx state management
2. Store, Actions, Reducers, Effects, Selectors
3. Модулі: Auth, User, FluxModels, Calendars, Orders, Cart, Admin
4. Поступова міграція

### Файли
- `src/Calendary.Ng/src/app/store/` (вся структура)

---

## GH-11: Email PDF та zip файли

**Джерело**: TASKS.md / GitHub #11
**Пріоритет**: P2 (Середній)
**Складність**: Середня
**Час**: 4-5 годин
**AI**: Claude Code

### Що треба зробити
1. Відправка PDF у друкарню після оплати
2. Email з PDF/ZIP attachments
3. Email про статус замовлення
4. HTML templates

### Файли
- `src/Calendary.Core/Services/EmailService.cs`
- `src/Calendary.Core/Templates/`
- `src/Calendary.Consumer/Consumers/OrderConsumer.cs`

---

## GH-3: Unit tests (розширені)

**Джерело**: TASKS.md / GitHub #3
**Пріоритет**: P2 (Середній)
**Складність**: Висока
**Час**: 10-15 годин
**AI**: Claude Code

**Примітка**: Epic 01 має task_30 для PDF тестів. Цей issue покриває ВСІ сервіси.

### Що треба зробити
1. xUnit test project
2. Тести для всіх Core Services:
   - ReplicateService
   - PdfService
   - FluxModelService
   - OrderService
   - WalletService
3. Moq для mocking
4. Coverage 70%+

### Файли
- `tests/Calendary.Core.Tests/` (вся структура)

---

**Створено**: 2025-11-15
