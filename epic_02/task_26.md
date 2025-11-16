# Task 26: Email notifications (SendGrid)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Claude
**Паралельно з**: Task 21, 22, 23, 24, 25

## Опис задачі

Інтегрувати SendGrid для відправки email notifications: підтвердження замовлення, статуси, ТТН, download links.

## Проблема

Користувачі повинні отримувати email повідомлення про важливі події: створення замовлення, оплата, відправка, tracking number.

## Що треба зробити

1. **Інтегрувати SendGrid**
   ```bash
   dotnet add package SendGrid
   ```
   - API Key в appsettings
   - SendGrid клієнт

2. **Створити Email Service**
   - `src/Calendary.Application/Services/EmailService.cs`
   - Send email method
   - Template rendering
   - Queue emails (background jobs)

3. **Email Templates**
   - HTML templates з брендингом
   - Responsive (mobile-friendly)
   - Templates:
     - Welcome (після реєстрації)
     - Order Confirmation (після створення замовлення)
     - Payment Confirmed (після успішної оплати)
     - Order Shipped (з ТТН)
     - Order Delivered
     - PDF Ready (для цифрового продукту)

4. **Template Engine**
   - Використовувати Razor або Handlebars
   - Dynamic data (order details, user name, tracking, etc.)
   - Вставляти order summary, tracking link

5. **Background Email Queue**
   - Використовувати Hangfire або RabbitMQ
   - Queue emails для async відправки
   - Retry logic при помилках
   - Email delivery status tracking

6. **Email Events**
   - Після реєстрації → Welcome email
   - Після створення Order → Order Confirmation
   - Після Payment → Payment Confirmed + PDF link (якщо digital)
   - Після створення ТТН → Order Shipped
   - Після delivery → Order Delivered (опціонально)

7. **Unsubscribe/Preferences**
   - Unsubscribe link в footer
   - Email preferences в profile
   - GDPR compliance

## Файли для створення/модифікації

- `src/Calendary.Core/Interfaces/IEmailService.cs`
- `src/Calendary.Application/Services/EmailService.cs`
- `src/Calendary.Infrastructure/Templates/Email/` - email templates
- `src/Calendary.Application/Jobs/SendEmailJob.cs`
- `src/Calendary.API/Controllers/EmailPreferencesController.cs`
- `appsettings.json` - SendGrid config

## Критерії успіху

- [ ] SendGrid інтегрований
- [ ] Email відправляються після ключових подій
- [ ] Templates виглядають професійно
- [ ] Emails responsive на mobile
- [ ] Background queue працює
- [ ] Retry logic працює при помилках
- [ ] Unsubscribe працює

## Залежності

Немає (незалежна задача)

## Технічні деталі

### appsettings.json
```json
{
  "SendGrid": {
    "ApiKey": "SG.xxx",
    "FromEmail": "noreply@calendary.com",
    "FromName": "Calendary"
  }
}
```

### EmailService.cs
```csharp
using SendGrid;
using SendGrid.Helpers.Mail;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string userName);
    Task SendOrderConfirmationAsync(string toEmail, Order order);
    Task SendPaymentConfirmedAsync(string toEmail, Order order);
    Task SendOrderShippedAsync(string toEmail, Order order, string trackingNumber);
    Task SendPdfReadyAsync(string toEmail, string calendarTitle, string downloadLink);
}

public class EmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly IConfiguration _config;
    private readonly IBackgroundJobClient _jobClient;

    public EmailService(IConfiguration config, IBackgroundJobClient jobClient)
    {
        _config = config;
        _client = new SendGridClient(config["SendGrid:ApiKey"]);
        _jobClient = jobClient;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = "Ласкаво просимо до Calendary! 🎉";
        var htmlContent = RenderWelcomeTemplate(userName);

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendOrderConfirmationAsync(string toEmail, Order order)
    {
        var subject = $"Замовлення #{order.OrderNumber} створено";
        var htmlContent = RenderOrderConfirmationTemplate(order);

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendPaymentConfirmedAsync(string toEmail, Order order)
    {
        var subject = $"Оплату підтверджено #{order.OrderNumber}";
        var htmlContent = RenderPaymentConfirmedTemplate(order);

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendOrderShippedAsync(string toEmail, Order order, string trackingNumber)
    {
        var subject = $"Замовлення #{order.OrderNumber} відправлено 📦";
        var htmlContent = RenderOrderShippedTemplate(order, trackingNumber);

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendPdfReadyAsync(string toEmail, string calendarTitle, string downloadLink)
    {
        var subject = $"Ваш календар \"{calendarTitle}\" готовий!";
        var htmlContent = RenderPdfReadyTemplate(calendarTitle, downloadLink);

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var from = new EmailAddress(
            _config["SendGrid:FromEmail"],
            _config["SendGrid:FromName"]
        );

        var to = new EmailAddress(toEmail);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

        // Queue email for background sending
        _jobClient.Enqueue(() => SendEmailBackgroundAsync(msg));
    }

    public async Task SendEmailBackgroundAsync(SendGridMessage message)
    {
        try
        {
            var response = await _client.SendEmailAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid error: {body}");
            }
        }
        catch (Exception ex)
        {
            // Log error
            throw;
        }
    }

    private string RenderWelcomeTemplate(string userName)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: #3b82f6; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background: #f9fafb; }}
                    .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Calendary</h1>
                    </div>
                    <div class='content'>
                        <h2>Вітаємо, {userName}!</h2>
                        <p>Дякуємо, що приєдналися до Calendary!</p>
                        <p>Тепер ви можете створювати унікальні календарі з вашими фотографіями.</p>
                        <p><a href='https://calendary.com/catalog' style='background: #3b82f6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Почати створення</a></p>
                    </div>
                    <div class='footer'>
                        <p>© 2025 Calendary. Всі права захищені.</p>
                        <p><a href='{{{{unsubscribe}}}}'>Відписатися</a></p>
                    </div>
                </div>
            </body>
            </html>
        ";
    }

    private string RenderOrderConfirmationTemplate(Order order)
    {
        var itemsList = string.Join("", order.Items.Select(item =>
            $"<li>{item.Calendar.Title} - {item.Format} - {item.Quantity}шт - {item.Price} грн</li>"
        ));

        return $@"
            <h2>Дякуємо за замовлення!</h2>
            <p>Номер замовлення: <strong>#{order.OrderNumber}</strong></p>
            <p>Статус: Очікує оплати</p>
            <h3>Товари:</h3>
            <ul>{itemsList}</ul>
            <p><strong>Загальна сума: {order.TotalAmount} грн</strong></p>
            <p><a href='https://calendary.com/payment/{order.Id}'>Перейти до оплати</a></p>
        ";
    }

    private string RenderOrderShippedTemplate(Order order, string trackingNumber)
    {
        return $@"
            <h2>Ваше замовлення відправлено!</h2>
            <p>Номер замовлення: <strong>#{order.OrderNumber}</strong></p>
            <p>Номер ТТН Нової Пошти: <strong>{trackingNumber}</strong></p>
            <p><a href='https://novaposhta.ua/tracking?cargo_number={trackingNumber}'>Відстежити посилку</a></p>
        ";
    }
}
```

## Примітки

- SendGrid безкоштовний до 100 emails/день
- HTML templates повинні бути responsive
- Background jobs запобігають блокуванню requests
- Unsubscribe важливий для GDPR

## Чому Claude?

Інтеграція з третьою стороною:
- SendGrid API integration
- Email template design
- Background job queuing
- Error handling та retry logic
- Event-driven architecture

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
