# Робоча папка Tech Lead Calendary

## Про роль

Tech Lead відповідає за технічний напрямок проєкту, архітектурні рішення, якість коду, менторинг розробників та технічну експертизу команди.

---

## Основні обов'язки

### 1. Технічне лідерство
- Визначення технічного стеку
- Архітектурні рішення
- Code review та якість коду
- Технічні стандарти та best practices
- Менторинг junior/middle розробників

### 2. Планування та оцінка
- Технічна декомпозиція User Stories
- Estimation складності тасок
- Виявлення технічних ризиків
- Capacity planning

### 3. Технічний борг
- Ідентифікація technical debt
- Приоритизація рефакторингу
- Balance: нові фічі vs tech debt
- Code quality metrics

### 4. Інтеграції та API
- Дизайн API endpoints
- Інтеграція з 3rd party (MonoBank, Replicate, Нова Пошта)
- Versioning API
- Documentation

### 5. Performance та масштабування
- Оптимізація швидкодії
- Database performance
- Caching стратегії
- Scalability planning

---

## Технічний стек Calendary

### Backend (.NET 9)

**Framework:** ASP.NET Core 9.0

**Архітектура:** Clean Architecture / Vertical Slices

```
src/
├── Calendary.Api/              # Web API layer
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
├── Calendary.Application/      # Business logic
│   ├── Features/               # Vertical slices
│   │   ├── Calendars/
│   │   ├── Orders/
│   │   ├── Payments/
│   │   └── AI/
│   ├── Common/
│   │   ├── Interfaces/
│   │   ├── Behaviours/
│   │   └── Exceptions/
│   └── Services/
├── Calendary.Domain/           # Domain entities
│   ├── Entities/
│   ├── ValueObjects/
│   └── Enums/
├── Calendary.Infrastructure/   # Data access, external services
│   ├── Persistence/
│   ├── Services/
│   │   ├── MonoBankService.cs
│   │   ├── ReplicateService.cs
│   │   └── NovaPoshtaService.cs
│   └── Messaging/
└── Calendary.Consumer/         # Background jobs (RabbitMQ)
    └── Handlers/
```

**Ключові NuGet packages:**
- Entity Framework Core 9.0
- MediatR (CQRS pattern)
- FluentValidation
- Serilog (logging)
- AutoMapper
- RabbitMQ.Client
- Polly (resilience)

### Frontend (Angular 20)

**Framework:** Angular 20 + Standalone Components

```
src/
├── app/
│   ├── core/                   # Singleton services
│   │   ├── auth/
│   │   ├── http/
│   │   └── services/
│   ├── shared/                 # Shared components
│   │   ├── components/
│   │   ├── directives/
│   │   └── pipes/
│   ├── features/               # Feature modules
│   │   ├── calendar-editor/
│   │   ├── templates/
│   │   ├── orders/
│   │   └── payment/
│   └── app.component.ts
```

**Ключові бібліотеки:**
- Angular Material (UI components)
- RxJS (reactive programming)
- NgRx (state management) - якщо потрібно
- Konva.js / Fabric.js (canvas editor)

### Database

**MSSQL Server 2022**

**Schema Design:**
```sql
-- Core tables
Users
Calendars (user designs)
CalendarTemplates (pre-made templates)
Orders
OrderItems
Payments
MonoBankTransactions

-- AI Integration
AIGeneratedImages
  - UserId
  - SourceImageUrl
  - GeneratedImageUrl
  - Style (watercolor, cartoon, etc.)
  - ReplicateJobId
  - Status
  - Cost
  - CreatedAt
```

**Indexes:**
- Composite indexes на частих запитах
- Covering indexes для performance
- Full-text search для пошуку шаблонів

### Infrastructure

**Messaging:** RabbitMQ
- Queue: `calendar-processing` (генерація PDF)
- Queue: `ai-generation` (Replicate jobs)
- Queue: `payment-notifications` (MonoBank webhooks)

**Storage:**
- Uploaded Images: `/calendary/uploads` (DigitalOcean volume)
- Generated PDFs: `/calendary/pdfs`
- Майбутнє: Azure Blob Storage

**Caching:**
- In-memory cache (production: Redis)
- Кешувати: templates, static data, AI results

---

## Архітектурні рішення

### 1. CQRS з MediatR

**Чому:**
- Розділення Commands (write) та Queries (read)
- Чистий код, легко тестувати
- Scalability (можна окремо scale reads vs writes)

**Приклад:**

```csharp
// Command
public record CreateCalendarCommand(
    Guid UserId,
    Guid TemplateId,
    List<CalendarImage> Images
) : IRequest<CreateCalendarResponse>;

public class CreateCalendarHandler : IRequestHandler<CreateCalendarCommand, CreateCalendarResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IStorageService _storage;

    public async Task<CreateCalendarResponse> Handle(
        CreateCalendarCommand request,
        CancellationToken cancellationToken)
    {
        // Business logic
        var calendar = new Calendar
        {
            UserId = request.UserId,
            TemplateId = request.TemplateId,
            Status = CalendarStatus.Draft
        };

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCalendarResponse(calendar.Id);
    }
}

// Query
public record GetCalendarQuery(Guid CalendarId) : IRequest<CalendarDto>;

public class GetCalendarHandler : IRequestHandler<GetCalendarQuery, CalendarDto>
{
    private readonly IApplicationDbContext _context;

    public async Task<CalendarDto> Handle(
        GetCalendarQuery request,
        CancellationToken cancellationToken)
    {
        var calendar = await _context.Calendars
            .Include(c => c.Template)
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == request.CalendarId, cancellationToken);

        return calendar.ToDto();
    }
}
```

### 2. Repository Pattern + Unit of Work

**Чому:**
- Абстракція над EF Core
- Легко тестувати (mock repository)
- Централізований data access

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork
{
    IRepository<Calendar> Calendars { get; }
    IRepository<Order> Orders { get; }
    Task<int> SaveChangesAsync();
}
```

### 3. API Versioning

**URL-based versioning:**
```
GET /api/v1/calendars
GET /api/v2/calendars  (майбутня версія)
```

**Implementation:**
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
```

### 4. Error Handling Strategy

**Global Exception Middleware:**

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedException(context, ex);
        }
    }
}
```

**Response format:**
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "Email": ["Email is required"],
    "Phone": ["Phone number is invalid"]
  }
}
```

---

## Інтеграції з 3rd Party

### 1. MonoBank Payment Integration

**API Documentation:** https://api.monobank.ua/docs/

**Flow:**
1. Створити invoice через API
2. Redirect користувача на MonoBank payment page
3. Отримати webhook про статус платежу
4. Оновити Order status

**Implementation:**

```csharp
public class MonoBankService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly string _merchantToken;

    public async Task<CreateInvoiceResponse> CreateInvoiceAsync(
        decimal amount,
        string orderId,
        string redirectUrl)
    {
        var request = new
        {
            amount = (int)(amount * 100), // копійки
            merchantPaymInfo = new
            {
                reference = orderId,
                destination = "Календар на замовлення"
            },
            redirectUrl = redirectUrl,
            webHookUrl = "https://calendary.com/api/webhooks/monobank"
        };

        var response = await _httpClient.PostAsJsonAsync("/api/merchant/invoice/create", request);
        return await response.Content.ReadFromJsonAsync<CreateInvoiceResponse>();
    }
}
```

**Webhook handling:**
```csharp
[HttpPost("webhooks/monobank")]
public async Task<IActionResult> MonoBankWebhook([FromBody] MonoBankWebhookDto webhook)
{
    // Verify signature
    if (!VerifySignature(webhook))
        return Unauthorized();

    // Update order
    await _mediator.Send(new UpdateOrderPaymentCommand(
        webhook.Reference,
        webhook.Status
    ));

    return Ok();
}
```

### 2. Replicate AI Integration

**API Documentation:** https://replicate.com/docs

**Models:**
- Stable Diffusion (img2img): `stability-ai/stable-diffusion`
- Background removal: `cjwbw/rembg`
- Upscaling: `nightmareai/real-esrgan`

**Implementation:**

```csharp
public class ReplicateService : IAIImageService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public async Task<string> GenerateArtisticImageAsync(
        string imageUrl,
        string style,
        CancellationToken cancellationToken)
    {
        var prediction = new
        {
            version = "stable-diffusion-version-hash",
            input = new
            {
                image = imageUrl,
                prompt = GetPromptForStyle(style),
                strength = 0.7
            }
        };

        var response = await _httpClient.PostAsJsonAsync("/v1/predictions", prediction);
        var result = await response.Content.ReadFromJsonAsync<PredictionResponse>();

        // Poll для результату (async job)
        return await PollForResult(result.Id, cancellationToken);
    }

    private string GetPromptForStyle(string style) => style switch
    {
        "watercolor" => "watercolor painting, artistic, beautiful, soft colors",
        "cartoon" => "cartoon style, animated, colorful, fun",
        "oil" => "oil painting, classical art, detailed",
        _ => "artistic, beautiful"
    };
}
```

**Async Processing з RabbitMQ:**

```csharp
// Producer (API)
public async Task RequestAIGeneration(Guid imageId, string style)
{
    var message = new AIGenerationRequest
    {
        ImageId = imageId,
        Style = style,
        RequestedAt = DateTime.UtcNow
    };

    await _messagePublisher.PublishAsync("ai-generation", message);
}

// Consumer (Background service)
public class AIGenerationConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channel.ConsumeAsync<AIGenerationRequest>(stoppingToken))
        {
            try
            {
                var result = await _replicateService.GenerateArtisticImageAsync(
                    message.ImageUrl,
                    message.Style,
                    stoppingToken
                );

                await UpdateImageWithResult(message.ImageId, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI generation failed for {ImageId}", message.ImageId);
                // Retry logic або Dead Letter Queue
            }
        }
    }
}
```

### 3. Нова Пошта API

**Endpoints:**
- Пошук відділень
- Розрахунок вартості доставки
- Створення ТТН

```csharp
public class NovaPoshtaService
{
    public async Task<List<WarehouseDto>> GetWarehousesAsync(string city)
    {
        var request = new
        {
            modelName = "Address",
            calledMethod = "getWarehouses",
            methodProperties = new { CityName = city }
        };

        // Call API
    }

    public async Task<string> CreateInternetDocumentAsync(Order order)
    {
        // Generate TTN
    }
}
```

---

## Code Quality Standards

### 1. Naming Conventions

**C# Backend:**
- Classes: `PascalCase`
- Methods: `PascalCase`
- Parameters/variables: `camelCase`
- Private fields: `_camelCase`
- Interfaces: `IPascalCase`
- Constants: `UPPER_CASE`

**TypeScript Frontend:**
- Classes/Interfaces: `PascalCase`
- Methods/variables: `camelCase`
- Components: `PascalCase`
- Services: `PascalCase` + `Service` suffix

### 2. Code Review Checklist

**Functionality:**
- [ ] Код робить те, що заявлено в таску
- [ ] Edge cases покриті
- [ ] Error handling присутній
- [ ] Немає hardcoded values (використовувати config)

**Code Quality:**
- [ ] Readable та maintainable
- [ ] Не дублюється (DRY principle)
- [ ] SOLID principles дотримані
- [ ] Proper naming

**Testing:**
- [ ] Unit tests написані
- [ ] Test coverage >80%
- [ ] Integration tests (якщо потрібно)

**Performance:**
- [ ] Немає N+1 queries
- [ ] Async/await правильно використовується
- [ ] Не блокуються threads

**Security:**
- [ ] Input validation
- [ ] No SQL injection vulnerabilities
- [ ] Secrets не в коді (використовувати env variables)
- [ ] Authorization checks

### 3. Testing Strategy

**Пірамі да тестів:**
```
        /\
       /  \  E2E Tests (5%)
      /____\
     /      \  Integration Tests (15%)
    /________\
   /          \  Unit Tests (80%)
  /____________\
```

**Unit Tests (xUnit):**

```csharp
public class CreateCalendarHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesCalendar()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var handler = new CreateCalendarHandler(mockContext.Object);
        var command = new CreateCalendarCommand(Guid.NewGuid(), Guid.NewGuid(), []);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

**Integration Tests:**

```csharp
public class CalendarsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    [Fact]
    public async Task GetCalendar_ReturnsOk()
    {
        // Arrange
        var calendarId = await CreateTestCalendar();

        // Act
        var response = await _client.GetAsync($"/api/v1/calendars/{calendarId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## Performance Optimization

### 1. Database Optimization

**Проблема:** N+1 query problem

```csharp
// ❌ Bad (N+1 queries)
var calendars = await _context.Calendars.ToListAsync();
foreach (var calendar in calendars)
{
    var images = await _context.CalendarImages.Where(i => i.CalendarId == calendar.Id).ToListAsync();
}

// ✅ Good (1 query)
var calendars = await _context.Calendars
    .Include(c => c.Images)
    .ToListAsync();
```

**Pagination:**

```csharp
public async Task<PagedResult<CalendarDto>> GetCalendarsAsync(int page, int pageSize)
{
    var query = _context.Calendars.AsQueryable();

    var total = await query.CountAsync();
    var items = await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<CalendarDto>
    {
        Items = items.Select(c => c.ToDto()).ToList(),
        Page = page,
        PageSize = pageSize,
        TotalCount = total
    };
}
```

### 2. Caching Strategy

**In-Memory Cache для static data:**

```csharp
public async Task<List<CalendarTemplateDto>> GetTemplatesAsync()
{
    const string cacheKey = "calendar-templates";

    if (!_cache.TryGetValue(cacheKey, out List<CalendarTemplateDto> templates))
    {
        templates = await _context.CalendarTemplates
            .Where(t => t.IsActive)
            .ToListAsync();

        _cache.Set(cacheKey, templates, TimeSpan.FromHours(1));
    }

    return templates;
}
```

**Redis для distributed cache (production):**

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "Calendary:";
});
```

### 3. API Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
```

---

## Security Best Practices

### 1. Authentication & Authorization

**JWT Tokens:**

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])
            )
        };
    });
```

**Authorization Policies:**

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CanEditCalendar", policy =>
        policy.RequireAssertion(context =>
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var calendarOwnerId = context.Resource as string;
            return userId == calendarOwnerId;
        })
    );
});
```

### 2. Input Validation

**FluentValidation:**

```csharp
public class CreateCalendarCommandValidator : AbstractValidator<CreateCalendarCommand>
{
    public CreateCalendarCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Images).NotNull().NotEmpty()
            .Must(images => images.Count <= 30)
            .WithMessage("Maximum 30 images allowed");
    }
}
```

### 3. Secrets Management

**Development:** User Secrets
```bash
dotnet user-secrets set "MonoBank:MerchantToken" "your-token"
```

**Production:** Environment Variables або Azure Key Vault

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["KeyVault:Url"]),
    new DefaultAzureCredential()
);
```

---

## Monitoring та Logging

### Structured Logging з Serilog

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Calendary")
    .WriteTo.Console()
    .WriteTo.File("/app/logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://seq:5341") // Optional
    .CreateLogger();

// Usage
_logger.LogInformation("Calendar {CalendarId} created by user {UserId}", calendarId, userId);
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRabbitMQ(builder.Configuration["RabbitMQ:ConnectionString"])
    .AddUrlGroup(new Uri("https://api.monobank.ua"), "MonoBank API");

app.MapHealthChecks("/health");
```

---

## Technical Debt Management

### Debt Register

```markdown
| ID | Description | Impact | Effort | Priority |
|----|-------------|--------|--------|----------|
| TD-01 | Refactor MonoBankService to use Polly | Medium | 2d | High |
| TD-02 | Add integration tests for payment flow | High | 3d | High |
| TD-03 | Migrate to .NET 10 | Low | 1d | Low |
| TD-04 | Optimize calendar rendering algorithm | Medium | 5d | Medium |
```

### Tech Debt Sprint

**Кожні 4-5 спринтів:**
- Виділити 1 повний спринт на tech debt
- Або 20-30% capacity кожного спринту

---

## Menторинг розробників

### Code Review Best Practices

**Давати constructive feedback:**

❌ Bad:
> "This code is bad"

✅ Good:
> "Consider extracting this logic into a separate service for better testability. Example: [code snippet]"

### 1-on-1 Sessions

**Щотижня 30 хв з кожним розробником:**
- Discuss blockers
- Career development
- Learning goals
- Feedback обопільний

### Knowledge Sharing

**Tech Talks (щотижня 30 хв):**
- Нові технології
- Best practices
- Code review insights
- Post-mortems

---

## Useful Resources

### Documentation
- .NET Documentation: https://learn.microsoft.com/en-us/dotnet/
- Angular Docs: https://angular.dev
- Clean Architecture: https://blog.cleancoder.com/

### Tools
- SonarQube (code quality)
- ReSharper / Rider
- Postman / Insomnia (API testing)

---

## Contacts

- **Scrum Master:** [ім'я]
- **DevOps:** [ім'я]
- **Developers:** [імена]
