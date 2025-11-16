# Реалізація системи кредитів - Документація

**Дата створення:** 2025-11-16
**Версія:** 1.0
**Статус:** Реалізовано

---

## Огляд

Реалізована система купівлі кредитів з інтеграцією Monobank для оплати. Кредити використовуються для оплати AI-генерації контенту (моделей та фото).

## Архітектура

### Backend (.NET)

#### 1. Моделі даних

**`Credit.cs`** - Кредити користувача
```csharp
- Id: int
- UserId: int
- Amount: int (кількість кредитів)
- Type: string (purchased, bonus, referral, admin)
- ExpiresAt: DateTime? (null для куплених)
- CreatedAt: DateTime
```

**`CreditTransaction.cs`** - Історія транзакцій
```csharp
- Id: int
- UserId: int
- Amount: int (+ зарахування, - списання)
- Type: string (fine_tuning, image_generation, purchase, bonus)
- Description: string
- OrderId: int?
- FluxModelId: int?
- CreditPackageId: int?
- CreatedAt: DateTime
```

**`CreditPackage.cs`** - Пакети кредитів для продажу
```csharp
- Id: int
- Name: string (Starter, Basic, Standard, Premium, Business)
- Credits: int (базові кредити)
- BonusCredits: int (бонусні кредити)
- PriceUAH: decimal
- IsActive: bool
- Description: string?
- DisplayOrder: int
- CreatedAt: DateTime
```

#### 2. EF Core Конфігурації

- `CreditConfiguration.cs` - налаштування Credit таблиці
- `CreditTransactionConfiguration.cs` - налаштування транзакцій
- `CreditPackageConfiguration.cs` - налаштування пакетів + seed data

**Seed пакетів:**
1. Starter: 100 кредитів, 0 бонусних, 100 грн
2. Basic: 300 + 20 бонусних, 300 грн (бонус 6.7%)
3. Standard: 500 + 50 бонусних, 500 грн (бонус 10%)
4. Premium: 1000 + 150 бонусних, 1000 грн (бонус 15%)
5. Business: 3000 + 600 бонусних, 3000 грн (бонус 20%)

#### 3. Сервіси

**`ICreditService`** / **`CreditService`**

Основні методи:
- `GetUserBalanceAsync(userId)` - отримати баланс
- `HasEnoughCreditsAsync(userId, amount)` - перевірка достатності
- `DeductCreditsAsync(...)` - списати кредити
- `AddCreditsAsync(...)` - додати кредити
- `GetActiveCreditPackagesAsync()` - список пакетів
- `ProcessCreditPackagePurchaseAsync(...)` - обробка оплати
- `AddWelcomeBonusAsync(userId)` - вітальний бонус (50 кредитів)
- `AddCreditsByAdminAsync(...)` - додавання адміном

**Вартість AI операцій:**
- Fine-tuning моделі: 145 кредитів
- Генерація фото (Flux): 14 кредитів
- Генерація фото (NanoBanana): 3 кредити

**`IPaymentService`** (розширено)

Додано метод:
- `CreateCreditPackageInvoiceAsync(userId, packageId, price, name)` - створення invoice для кредитів

#### 4. API Контролери

**`CreditsController`** (`/api/credits`)

Endpoints:
- `GET /balance` - баланс користувача
- `GET /packages` - список пакетів
- `POST /purchase` - купівля пакету
- `GET /transactions` - історія транзакцій
- `GET /check?amount=X` - перевірка балансу

**`AdminCreditsController`** (`/api/admin/credits`)

Endpoints (тільки для адмінів):
- `POST /add` - додати кредити користувачу
- `GET /balance/{userId}` - баланс користувача
- `GET /transactions/{userId}` - транзакції користувача

**`PaymentController`** (оновлено)

Webhook обробка:
- Додано обробку `CreditPackageId` в `MonoCallback`
- При успішній оплаті викликається `ProcessCreditPackagePurchaseAsync`

### Frontend (Angular)

#### 1. Сервіси

**`CreditService`** (`src/services/credit.service.ts`)

Методи:
- `getBalance()` - баланс
- `getPackages()` - пакети
- `purchasePackage(id)` - купівля
- `getTransactions()` - історія
- `checkBalance(amount)` - перевірка

#### 2. Компоненти

**`CreditBalanceComponent`**
- Віджет відображення балансу кредитів
- Показує: загальний баланс, куплені, бонусні
- Попередження про кредити що закінчуються
- Кнопка "Купити кредити"

**`CreditsShopComponent`**
- Сторінка магазину кредитів
- Grid пакетів з вказанням бонусів
- Розрахунок скільки моделей/фото можна згенерувати
- Купівля через Monobank

## Використання кредитів

### Перевірка перед генерацією

При генерації моделі або фото:

```csharp
// Перевірка балансу
var hasEnough = await _creditService.HasEnoughCreditsAsync(userId, COST_FINE_TUNING);
if (!hasEnough)
{
    return BadRequest(new { message = "Insufficient credits" });
}

// Списання кредитів
await _creditService.DeductCreditsAsync(
    userId,
    COST_FINE_TUNING,
    "fine_tuning",
    $"Fine-tuning model {modelId}",
    fluxModelId: modelId
);
```

### Оплата календаря кредитами

Можливість оплати календаря кредитами (як альтернатива грошовій оплаті):

```csharp
// При оформленні замовлення
if (paymentMethod == "credits")
{
    var calendarCost = 200; // вартість в кредитах
    await _creditService.DeductCreditsAsync(
        userId,
        calendarCost,
        "calendar_purchase",
        $"Purchase calendar #{orderId}",
        orderId: orderId
    );
}
```

## Monobank інтеграція

### Створення invoice

```csharp
var paymentUrl = await _paymentService.CreateCreditPackageInvoiceAsync(
    userId,
    package.Id,
    package.PriceUAH,
    package.Name
);
// Redirect користувача на paymentUrl
```

### Webhook обробка

При успішній оплаті Monobank надсилає webhook:

1. `PaymentController.MonoCallback` отримує webhook
2. Знаходить `PaymentInfo` по `InvoiceId`
3. Якщо `CreditPackageId != null`:
   - Викликає `CreditService.ProcessCreditPackagePurchaseAsync`
   - Додає куплені кредити (type: "purchased", ExpiresAt: null)
   - Додає бонусні кредити (type: "bonus", ExpiresAt: +1 year)
   - Записує транзакції

## Вітальні бонуси

При реєстрації нового користувача:

```csharp
await _creditService.AddWelcomeBonusAsync(userId);
// Додає 50 кредитів (type: "welcome_bonus", ExpiresAt: +1 year)
```

## Адміністрування

Адмін може додавати кредити користувачу через API:

```http
POST /api/admin/credits/add
{
  "userId": 123,
  "amount": 500,
  "reason": "Compensation for issue #456"
}
```

## База даних

### Міграція

Для застосування змін потрібно створити міграцію:

```bash
dotnet ef migrations add AddCreditsSystem --project src/Calendary.Repos --startup-project src/Calendary.Api
dotnet ef database update --project src/Calendary.Repos --startup-project src/Calendary.Api
```

### Таблиці

- `Credits` - кредити користувачів
- `CreditTransactions` - історія транзакцій
- `CreditPackages` - пакети для продажу
- `PaymentInfos` - оновлено (додано `UserId`, `CreditPackageId`)

## Dependency Injection

Реєстрація в `DependencyRegistration.cs`:

```csharp
services.AddScoped<ICreditService, CreditService>();
```

## Приклади використання

### 1. Купівля пакету кредитів

```typescript
// Angular
this.creditService.purchasePackage(packageId).subscribe(response => {
  window.location.href = response.paymentUrl; // Redirect to Monobank
});
```

### 2. Перевірка балансу перед генерацією

```csharp
// Backend
var balance = await _creditService.GetUserBalanceAsync(userId);
if (balance.Total < 145) {
    return BadRequest("Not enough credits for fine-tuning");
}
```

### 3. Відображення балансу

```html
<!-- Angular component -->
<app-credit-balance></app-credit-balance>
```

## Константи

**Вартість операцій (CreditService):**
```csharp
private const int COST_FINE_TUNING = 145;
private const int COST_IMAGE_FLUX = 14;
private const int COST_IMAGE_NANOBANANA = 3;
private const int WELCOME_BONUS = 50;
```

## Безпека

1. **Перевірка балансу** - завжди перевіряти перед списанням
2. **Транзакції** - використання DB transactions для атомарності
3. **Webhook validation** - валідація X-Sign від Monobank (якщо потрібно)
4. **Authorization** - endpoints захищені [Authorize] атрибутом
5. **Admin endpoints** - тільки для ролі Admin

## Тестування

### Ручне тестування

1. Зареєструвати нового користувача → перевірити вітальний бонус
2. Купити пакет кредитів → перевірити webhook обробку
3. Згенерувати модель → перевірити списання 145 кредитів
4. Перевірити баланс через API та UI

### Unit тести

Створити тести для:
- `CreditService.DeductCreditsAsync` - списання
- `CreditService.ProcessCreditPackagePurchaseAsync` - обробка оплати
- `CreditService.GetUserBalanceAsync` - розрахунок балансу

## Подальші покращення

1. ✅ Реалізована базова система
2. 🔄 Додати реферальну програму
3. 🔄 Програма лояльності (бонуси за кожне N-те замовлення)
4. 🔄 Subscription модель (місячна підписка з кредитами)
5. 🔄 Gift credits (передача кредитів іншому користувачу)
6. 🔄 Expiration notifications (email за 7 днів до згорання бонусів)

## Документація для користувачів

### Що таке кредити?

Кредити - внутрішня валюта для оплати AI-генерації:
- **Створення моделі:** 145 кредитів
- **Генерація фото (Flux):** 14 кредитів
- **Генерація фото (NanoBanana):** 3 кредити

### Чому NanoBanana дешевше?

NanoBanana - швидша модель з нижчою вартістю, але якість трохи нижча ніж у Flux.

### Чи згорають кредити?

- **Куплені кредити:** безстрокові
- **Бонусні кредити:** дійсні 12 місяців

### Пріоритет списання

Спочатку списуються бонусні кредити (що скоро закінчуються), потім куплені.

---

**Автор:** AI Team
**Дата:** 2025-11-16
**Версія:** 1.0
