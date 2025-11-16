# Task 30: Security audit та fixes

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 5-6 годин
**Відповідальний AI**: Claude
**Залежить від**: Всі попередні

## Опис задачі

Провести security audit Customer Portal, перевірити на XSS, CSRF, SQL injection, rate limiting, виправити знайдені вразливості.

## Проблема

Security критично важлива для e-commerce платформи з платежами та персональними даними користувачів.

## Що треба зробити

1. **XSS (Cross-Site Scripting) Protection**
   - Перевірити всі user inputs
   - Sanitize HTML content
   - React автоматично escapes (verify)
   - Перевірити dangerouslySetInnerHTML usage
   - Content Security Policy headers

2. **CSRF (Cross-Site Request Forgery) Protection**
   - CSRF tokens для forms
   - SameSite cookies
   - Verify Origin header
   - Anti-CSRF middleware

3. **SQL Injection Protection**
   - EF Core parameterized queries (verify)
   - No raw SQL
   - Input validation
   - Prepared statements

4. **Authentication Security**
   - Password requirements (min 8 chars, complexity)
   - BCrypt hashing (verify)
   - JWT token security
   - Token expiration
   - Refresh token rotation
   - Account lockout після failed attempts

5. **Authorization Checks**
   - Verify ownership перед operations
   - Role-based access control
   - API endpoints authorization
   - File access control

6. **Rate Limiting**
   - Login attempts (5 per 15 min)
   - API requests (100 per min)
   - File uploads (10 per hour)
   - Email sending
   - DDoS protection

7. **Data Protection**
   - HTTPS everywhere
   - Sensitive data encryption
   - PII (Personal Identifiable Information) handling
   - GDPR compliance
   - Secure cookies (HttpOnly, Secure)

8. **File Upload Security**
   - File type validation
   - File size limits
   - Virus scanning (опціонально)
   - Safe file storage
   - No executable files

9. **Dependency Security**
   - npm audit
   - Snyk scan
   - Update vulnerable packages
   - Regular security updates

10. **Security Headers**
    - Content-Security-Policy
    - X-Frame-Options
    - X-Content-Type-Options
    - Strict-Transport-Security
    - Referrer-Policy

## Файли для створення/модифікації

### Backend
- `src/Calendary.API/Middleware/RateLimitingMiddleware.cs`
- `src/Calendary.API/Middleware/SecurityHeadersMiddleware.cs`
- `src/Calendary.API/Filters/ValidateAntiForgeryTokenAttribute.cs`
- `Program.cs` - security config
- `appsettings.json` - security settings

### Frontend
- `next.config.js` - CSP headers
- `src/lib/sanitize.ts` - input sanitization
- `src/middleware.ts` - security checks

## Критерії успіху

- [ ] No XSS vulnerabilities
- [ ] CSRF protection enabled
- [ ] No SQL injection vulnerabilities
- [ ] Rate limiting працює
- [ ] Security headers налаштовані
- [ ] Authentication secure
- [ ] Authorization checks на місці
- [ ] File uploads secure
- [ ] npm audit clean
- [ ] HTTPS enforced

## Залежності

- Всі features повинні бути імплементовані

## Технічні деталі

### Rate Limiting Middleware
```csharp
using AspNetCoreRateLimit;

// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100
        },
        new RateLimitRule
        {
            Endpoint = "*/api/auth/login",
            Period = "15m",
            Limit = 5
        }
    };
});

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

app.UseIpRateLimiting();
```

### Security Headers Middleware
```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // CSP
        context.Response.Headers.Add(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self' https://api.novaposhta.ua; " +
            "frame-ancestors 'none';"
        );

        // X-Frame-Options
        context.Response.Headers.Add("X-Frame-Options", "DENY");

        // X-Content-Type-Options
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

        // Strict-Transport-Security
        context.Response.Headers.Add(
            "Strict-Transport-Security",
            "max-age=31536000; includeSubDomains"
        );

        // Referrer-Policy
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

        // X-XSS-Protection
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

        await _next(context);
    }
}
```

### CSRF Protection
```csharp
// Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-X-CSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Controller
[ValidateAntiForgeryToken]
[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
{
    // ...
}
```

### Input Validation
```csharp
public class CreateCalendarRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯіІїЇєЄ0-9\s\-]+$")]
    public string Title { get; set; }

    [Required]
    [JsonValidation] // Custom validator
    public string DesignData { get; set; }
}

public class JsonValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value is string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult("Invalid JSON format");
            }
        }
        return new ValidationResult("Value must be a string");
    }
}
```

### Secure File Upload
```csharp
public async Task<IActionResult> UploadFile(IFormFile file)
{
    // File size check
    if (file.Length > 10 * 1024 * 1024) // 10MB
        return BadRequest("File too large");

    // File type check
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(extension))
        return BadRequest("Invalid file type");

    // MIME type check
    var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!allowedMimeTypes.Contains(file.ContentType))
        return BadRequest("Invalid MIME type");

    // Scan for malware (optional - requires ClamAV or similar)
    // var isSafe = await _virusScanner.ScanAsync(file);
    // if (!isSafe) return BadRequest("File contains malware");

    // Generate secure filename
    var safeFileName = $"{Guid.NewGuid()}{extension}";

    // Save file
    var path = Path.Combine(_config["FileStorage:UploadPath"], safeFileName);
    using (var stream = new FileStream(path, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    return Ok(new { url = $"/uploads/{safeFileName}" });
}
```

### Frontend: CSP Headers (Next.js)
```javascript
// next.config.js
const nextConfig = {
  async headers() {
    return [
      {
        source: '/:path*',
        headers: [
          {
            key: 'Content-Security-Policy',
            value: [
              "default-src 'self'",
              "script-src 'self' 'unsafe-eval' 'unsafe-inline'",
              "style-src 'self' 'unsafe-inline'",
              "img-src 'self' data: https:",
              "font-src 'self' data:",
              "connect-src 'self' https://api.calendary.com",
              "frame-ancestors 'none'",
            ].join('; '),
          },
          {
            key: 'X-Frame-Options',
            value: 'DENY',
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff',
          },
        ],
      },
    ]
  },
}
```

### Dependency Security
```bash
# Check for vulnerabilities
npm audit

# Fix automatically
npm audit fix

# Or use Snyk
npx snyk test
npx snyk wizard
```

## Примітки

- Security - це не one-time task, а continuous process
- Regular updates важливі
- Penetration testing рекомендується
- OWASP Top 10 - must know

## Чому Claude?

Security-critical задача:
- Security vulnerability assessment
- Attack vector analysis
- Mitigation strategies
- Security best practices
- Compliance requirements (GDPR)

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
