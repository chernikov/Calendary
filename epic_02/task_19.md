# Task 19: Інтеграція MonoBank (платежі)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 6-8 годин
**Відповідальний AI**: Claude
**Залежить від**: Task 18 (Checkout форма)
**Паралельно з**: Task 20 (Nova Poshta)

## Опис задачі

Інтегрувати MonoBank Acquiring API для прийому онлайн платежів за календарі. Реалізувати створення invoices, обробку webhooks, підтвердження платежів та error handling.

## Проблема

Користувачі повинні мати можливість оплачувати замовлення онлайн через MonoBank, з автоматичним підтвердженням оплати та оновленням статусу замовлення.

## Передумови

1. **Отримати MonoBank Merchant Token**
   - Зареєструватися в MonoBank для бізнесу
   - Отримати API токен для acquiring
   - Налаштувати webhook URL

2. **Документація MonoBank API**
   - [https://api.monobank.ua/docs/](https://api.monobank.ua/docs/)
   - Acquiring API: створення invoice, webhook events

## Що треба зробити

### 1. Backend: Payment Service

**IPaymentService.cs:**
```csharp
public interface IPaymentService
{
    Task<CreateInvoiceResponse> CreateInvoiceAsync(int orderId, decimal amount, string description);
    Task<PaymentStatus> GetPaymentStatusAsync(string invoiceId);
    Task<bool> ProcessWebhookAsync(MonoBankWebhook webhook);
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Success,
    Failed,
    Reversed
}
```

**MonoBankService.cs:**
```csharp
public class MonoBankService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IOrderRepository _orderRepo;
    private readonly ILogger<MonoBankService> _logger;

    private string MonoBankApiUrl => "https://api.monobank.ua/api/merchant";
    private string Token => _config["MonoBank:Token"];
    private string WebhookUrl => _config["MonoBank:WebhookUrl"];

    public async Task<CreateInvoiceResponse> CreateInvoiceAsync(
        int orderId,
        decimal amount,
        string description)
    {
        // 1. Отримати order з БД
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null) throw new NotFoundException("Order not found");

        // 2. Створити invoice в MonoBank
        var request = new
        {
            amount = (int)(amount * 100), // MonoBank приймає копійки
            ccy = 980, // UAH код валюти
            merchantPaymInfo = new
            {
                reference = $"ORDER-{orderId}",
                destination = description,
                basketOrder = new[]
                {
                    new
                    {
                        name = "Calendar Order",
                        qty = 1,
                        sum = (int)(amount * 100),
                        code = $"CAL-{orderId}"
                    }
                }
            },
            redirectUrl = $"{_config["App:FrontendUrl"]}/orders/{orderId}/payment-success",
            webHookUrl = WebhookUrl,
            validity = 3600, // 1 година
            paymentType = "debit"
        };

        _httpClient.DefaultRequestHeaders.Add("X-Token", Token);

        var response = await _httpClient.PostAsJsonAsync(
            $"{MonoBankApiUrl}/invoice/create",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("MonoBank create invoice failed: {Error}", error);
            throw new PaymentException("Failed to create MonoBank invoice");
        }

        var result = await response.Content.ReadFromJsonAsync<MonoBankInvoiceResponse>();

        // 3. Зберегти invoice ID в Order
        order.PaymentInvoiceId = result.InvoiceId;
        order.PaymentUrl = result.PageUrl;
        order.PaymentStatus = PaymentStatus.Pending;
        await _orderRepo.UpdateAsync(order);

        return new CreateInvoiceResponse
        {
            InvoiceId = result.InvoiceId,
            PaymentUrl = result.PageUrl
        };
    }

    public async Task<bool> ProcessWebhookAsync(MonoBankWebhook webhook)
    {
        try
        {
            // 1. Валідувати webhook (signature check якщо є)
            // MonoBank не передає signature, але можна валідувати IP

            // 2. Знайти order по invoiceId
            var order = await _orderRepo.GetByPaymentInvoiceIdAsync(webhook.InvoiceId);
            if (order == null)
            {
                _logger.LogWarning("Order not found for invoiceId: {InvoiceId}", webhook.InvoiceId);
                return false;
            }

            // 3. Оновити статус оплати
            switch (webhook.Status)
            {
                case "success":
                    order.PaymentStatus = PaymentStatus.Success;
                    order.OrderStatus = OrderStatus.Paid;
                    order.PaidAt = DateTime.UtcNow;

                    // Trigger email notification
                    await _notificationService.SendOrderConfirmationAsync(order.Id);
                    break;

                case "failure":
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.OrderStatus = OrderStatus.PaymentFailed;
                    break;

                case "reversed":
                    order.PaymentStatus = PaymentStatus.Reversed;
                    order.OrderStatus = OrderStatus.Cancelled;
                    break;

                default:
                    _logger.LogWarning("Unknown webhook status: {Status}", webhook.Status);
                    return false;
            }

            await _orderRepo.UpdateAsync(order);

            // 4. Зберегти webhook в audit log
            await SaveWebhookLog(webhook, order.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MonoBank webhook");
            return false;
        }
    }
}
```

### 2. Webhook Controller

**WebhooksController.cs:**
```csharp
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<WebhooksController> _logger;

    // POST /api/webhooks/monobank
    [HttpPost("monobank")]
    public async Task<IActionResult> MonoBankWebhook([FromBody] MonoBankWebhook webhook)
    {
        _logger.LogInformation("Received MonoBank webhook: {InvoiceId}, Status: {Status}",
            webhook.InvoiceId, webhook.Status);

        try
        {
            var result = await _paymentService.ProcessWebhookAsync(webhook);

            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to process webhook");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MonoBank webhook handler");
            return StatusCode(500);
        }
    }
}
```

**MonoBankWebhook.cs:**
```csharp
public class MonoBankWebhook
{
    [JsonPropertyName("invoiceId")]
    public string InvoiceId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // success, failure, reversed

    [JsonPropertyName("amount")]
    public int Amount { get; set; } // в копійках

    [JsonPropertyName("ccy")]
    public int Currency { get; set; } // 980 = UAH

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("modifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }
}
```

### 3. Frontend: Payment Flow

**services/paymentService.ts:**
```typescript
import axios from 'axios';

export interface CreatePaymentRequest {
  orderId: number;
  amount: number;
  description: string;
}

export interface CreatePaymentResponse {
  invoiceId: string;
  paymentUrl: string;
}

export class PaymentService {
  private static API_URL = process.env.NEXT_PUBLIC_API_URL;

  static async createPayment(request: CreatePaymentRequest): Promise<CreatePaymentResponse> {
    const response = await axios.post<CreatePaymentResponse>(
      `${this.API_URL}/payments/create`,
      request
    );
    return response.data;
  }

  static async getPaymentStatus(invoiceId: string): Promise<string> {
    const response = await axios.get(`${this.API_URL}/payments/${invoiceId}/status`);
    return response.data.status;
  }

  static redirectToPayment(paymentUrl: string) {
    window.location.href = paymentUrl;
  }
}
```

**app/checkout/page.tsx (частина):**
```typescript
const handlePayment = async () => {
  try {
    setLoading(true);

    // 1. Створити замовлення
    const order = await OrderService.createOrder({
      items: cartItems,
      shippingAddress,
      // ...
    });

    // 2. Створити invoice в MonoBank
    const payment = await PaymentService.createPayment({
      orderId: order.id,
      amount: order.totalAmount,
      description: `Замовлення календарів #${order.id}`,
    });

    // 3. Redirect на сторінку оплати MonoBank
    PaymentService.redirectToPayment(payment.paymentUrl);
  } catch (error) {
    console.error('Payment error:', error);
    toast.error('Помилка при створенні платежу');
  } finally {
    setLoading(false);
  }
};
```

### 4. Payment Success/Failure pages

**app/orders/[id]/payment-success/page.tsx:**
```typescript
'use client';

import { useEffect, useState } from 'use';
import { useParams } from 'next/navigation';
import { OrderService } from '@/services/orderService';

export default function PaymentSuccessPage() {
  const params = useParams();
  const orderId = params.id as string;
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkPaymentStatus = async () => {
      try {
        // Перевірити статус замовлення
        const orderData = await OrderService.getById(orderId);
        setOrder(orderData);

        if (orderData.paymentStatus === 'Success') {
          // Успішна оплата
          // Показати success message
        } else {
          // Очікувати webhook від MonoBank
          // Можна додати polling
        }
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };

    checkPaymentStatus();
  }, [orderId]);

  if (loading) return <div>Перевірка статусу оплати...</div>;

  return (
    <div className="container mx-auto py-12">
      <h1>Дякуємо за замовлення!</h1>
      <p>Номер замовлення: {orderId}</p>
      {/* Деталі замовлення */}
    </div>
  );
}
```

### 5. Database updates

**Додати поля до Order:**
```csharp
public class Order
{
    // ... existing fields

    public string? PaymentInvoiceId { get; set; }
    public string? PaymentUrl { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
}
```

**EF Core міграція:**
```bash
dotnet ef migrations add AddPaymentFieldsToOrder
dotnet ef database update
```

### 6. Configuration

**appsettings.json:**
```json
{
  "MonoBank": {
    "Token": "", // з environment variable
    "WebhookUrl": "https://calendary.com.ua/api/webhooks/monobank",
    "ApiUrl": "https://api.monobank.ua/api/merchant"
  },
  "App": {
    "FrontendUrl": "https://calendary.com.ua"
  }
}
```

**.env (для frontend):**
```
NEXT_PUBLIC_API_URL=https://api.calendary.com.ua/api
```

### 7. Testing

**Sandbox тестування:**
- MonoBank має sandbox environment
- Тестові картки для різних сценаріїв

**Integration tests:**
```csharp
public class MonoBankServiceTests
{
    [Fact]
    public async Task CreateInvoice_ValidOrder_ReturnsPaymentUrl()
    {
        // Arrange
        // Act
        // Assert
    }

    [Fact]
    public async Task ProcessWebhook_SuccessStatus_UpdatesOrderStatus()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

## Критерії успіху

- [x] MonoBank API інтеграція реалізована
- [x] Створення invoice працює
- [x] Webhook обробка працює
- [x] Order status оновлюється після оплати
- [x] Webhook signature validation увімкнено
- [x] Error handling для failed payments (failure, reversed)
- [x] Redirect на payment URL працює
- [x] Payment success/failure UI створено (в order component)
- [x] Payment status polling реалізовано
- [x] Database schema оновлено (PaymentInfo.UserId додано)
- [ ] Integration tests написані (TODO для майбутнього)

## Security considerations

1. **Webhook validation**
   - Валідувати IP адреси MonoBank
   - Логувати всі webhook calls
   - Idempotency (не обробляти той самий webhook двічі)

2. **Sensitive data**
   - Merchant Token в environment variables (НІКОЛИ в коді)
   - HTTPS обов'язковий для webhook URL
   - PCI DSS compliance (не зберігати card data)

3. **Error handling**
   - Не expose sensitive errors користувачам
   - Логувати всі payment errors
   - Retry logic для transient failures

## Залежності

- Task 18: Checkout форма (звідти йде create order)

## Блокує наступні задачі

- Task 23: Історія замовлень (потребує payment status)

## Примітки

### Чому Claude?

Ця задача потребує:
- **Security expertise**: робота з платежами
- **Інтеграція третіх сторін**: складний API
- **Error handling**: багато edge cases
- **Webhook processing**: асинхронна логіка
- **Testing**: критична функціональність

Claude краще справляється з такими критичними інтеграціями.

### MonoBank limits

- Invoice validity: максимум 24 години
- Amount: мінімум 1 грн, максимум залежить від мерчанта
- Webhook retries: MonoBank повторює webhook до 10 разів

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: 2025-11-16

## Зміни та покращення

### Backend
1. **Webhook signature validation** - Увімкнено перевірку підпису X-Sign від MonoBank (PaymentController.cs:100)
2. **Payment failure handling** - Додано обробку статусів "failure" і "reversed" у webhook callback (PaymentController.cs:145-198)
3. **UserId tracking** - Додано поле UserId до PaymentInfo для відстеження користувача, що створив платіж (PaymentInfo.cs:29-30)
4. **Order.Status updates** - При неуспішній оплаті статус змінюється на "PaymentFailed" або "Cancelled"

### Frontend
1. **Payment status polling** - Реалізовано автоматичне опитування статусу замовлення кожні 5 секунд протягом 5 хвилин (OrderComponent.ts:72-98)
2. **Payment messages** - Додано UI для відображення статусу оплати з іконками та спінером (OrderComponent.html:6-22)
3. **Query parameter handling** - При поверненні з MonoBank додається параметр ?payment=processing для ініціалізації polling

### Що працює
- MonoBank invoice створення для замовлень, моделей та пакетів кредитів
- Webhook обробка з валідацією підпису
- Автоматичне оновлення Order.IsPaid та Order.Status
- Обробка успішних, невдалих та скасованих платежів
- Frontend polling для real-time оновлення статусу
- Audit trail через MonoWebhookEvent

### Що потребує уваги в майбутньому
- **Database migration** - Потрібно створити міграцію для PaymentInfo.UserId (dotnet ef migrations add)
- **Integration tests** - Створити тести для payment flows
- **Email notifications** - Інтеграція з email service для підтвердження оплати (згадується в task але не реалізовано)
