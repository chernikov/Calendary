# Робоча папка Developer Calendary

## Про роль

Developer відповідає за розробку функціоналу, написання якісного коду, тестування, підтримку документації та співпрацю з командою для досягнення Sprint Goals.

---

## Основні обов'язки

### 1. Розробка функціоналу
- Імплементація User Stories
- Написання чистого, підтримуваного коду
- Дотримання code standards
- Рефакторинг існуючого коду

### 2. Тестування
- Написання unit tests
- Написання integration tests
- Manual testing власного коду
- Підтримка test coverage >80%

### 3. Code Review
- Review коду колег
- Надання constructive feedback
- Обговорення архітектурних рішень

### 4. Документація
- Коментарі в коді (де потрібно)
- API documentation
- README для нових модулів
- Technical specs

### 5. Співпраця
- Участь у Daily Standup
- Sprint Planning та Estimation
- Pair programming (при потребі)
- Knowledge sharing

---

## Tech Stack

### Backend Development (.NET 9)

**Languages:** C# 12

**Frameworks:**
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- MediatR
- FluentValidation

**Tools:**
- Visual Studio 2022 / Rider
- SQL Server Management Studio
- Postman
- Git

### Frontend Development (Angular 19)

**Languages:** TypeScript 5.x

**Frameworks:**
- Angular 19 (standalone components)
- Angular Material
- RxJS
- Konva.js (calendar editor)

**Tools:**
- VS Code
- Angular CLI
- Chrome DevTools
- npm

---

## Development Workflow

### 1. Отримання таски

**З Jira/Azure DevOps:**
```
US-101: Інтеграція Replicate API для AI генерації

Description:
As a user, I want to transform my photos into artistic styles
so that my calendar looks unique and professional

Acceptance Criteria:
- [ ] Користувач може вибрати один з 5 стилів
- [ ] Генерація займає <60 секунд
- [ ] Показується progress indicator
- [ ] Можливість regenerate
- [ ] Обробка помилок (якщо API недоступний)

Story Points: 8
```

### 2. Створення branch

```bash
# Feature branch naming: feature/US-{number}-short-description
git checkout develop
git pull origin develop
git checkout -b feature/US-101-replicate-integration
```

### 3. Розробка

**Backend Example:**

```csharp
// 1. Створити Service
namespace Calendary.Infrastructure.Services;

public interface IReplicateService
{
    Task<string> GenerateArtisticImageAsync(string imageUrl, string style, CancellationToken cancellationToken);
}

public class ReplicateService : IReplicateService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReplicateService> _logger;

    public ReplicateService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ReplicateService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.replicate.com");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_configuration["Replicate:ApiKey"]}");
    }

    public async Task<string> GenerateArtisticImageAsync(
        string imageUrl,
        string style,
        CancellationToken cancellationToken)
    {
        try
        {
            var prediction = new
            {
                version = _configuration["Replicate:ModelVersion"],
                input = new
                {
                    image = imageUrl,
                    prompt = GetPromptForStyle(style),
                    strength = 0.7
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/v1/predictions", prediction, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PredictionResponse>(cancellationToken);

            // Poll for result
            return await PollForResultAsync(result.Id, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call Replicate API");
            throw new ExternalServiceException("AI generation service is unavailable", ex);
        }
    }

    private async Task<string> PollForResultAsync(string predictionId, CancellationToken cancellationToken)
    {
        var maxAttempts = 60; // 60 seconds
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            var response = await _httpClient.GetAsync($"/v1/predictions/{predictionId}", cancellationToken);
            var prediction = await response.Content.ReadFromJsonAsync<PredictionResponse>(cancellationToken);

            if (prediction.Status == "succeeded")
                return prediction.Output;

            if (prediction.Status == "failed")
                throw new ExternalServiceException("AI generation failed");

            await Task.Delay(1000, cancellationToken); // Wait 1 second
            attempt++;
        }

        throw new TimeoutException("AI generation timeout");
    }

    private string GetPromptForStyle(string style) => style switch
    {
        "watercolor" => "watercolor painting, artistic, beautiful, soft colors, professional",
        "cartoon" => "cartoon style, animated, colorful, fun, digital art",
        "oil" => "oil painting, classical art, detailed, masterpiece",
        "sketch" => "pencil sketch, black and white, detailed drawing",
        "modern" => "modern art, abstract, vibrant colors, contemporary",
        _ => "artistic, beautiful, professional"
    };
}

// 2. Зареєструвати в DI
builder.Services.AddHttpClient<IReplicateService, ReplicateService>();

// 3. Створити Command/Query
public record GenerateArtisticImageCommand(
    Guid ImageId,
    string ImageUrl,
    string Style
) : IRequest<GenerateArtisticImageResponse>;

public class GenerateArtisticImageHandler : IRequestHandler<GenerateArtisticImageCommand, GenerateArtisticImageResponse>
{
    private readonly IReplicateService _replicateService;
    private readonly IApplicationDbContext _context;

    public GenerateArtisticImageHandler(
        IReplicateService replicateService,
        IApplicationDbContext context)
    {
        _replicateService = replicateService;
        _context = context;
    }

    public async Task<GenerateArtisticImageResponse> Handle(
        GenerateArtisticImageCommand request,
        CancellationToken cancellationToken)
    {
        var resultUrl = await _replicateService.GenerateArtisticImageAsync(
            request.ImageUrl,
            request.Style,
            cancellationToken
        );

        var aiImage = new AIGeneratedImage
        {
            SourceImageId = request.ImageId,
            GeneratedImageUrl = resultUrl,
            Style = request.Style,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIGeneratedImages.Add(aiImage);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenerateArtisticImageResponse(aiImage.Id, resultUrl);
    }
}

// 4. Створити Controller endpoint
[ApiController]
[Route("api/v1/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateArtisticImageResponse>> Generate(
        [FromBody] GenerateArtisticImageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GenerateArtisticImageCommand(
            request.ImageId,
            request.ImageUrl,
            request.Style
        );

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
```

**Frontend Example:**

```typescript
// 1. Створити Service
@Injectable({ providedIn: 'root' })
export class AIService {
  private apiUrl = environment.apiUrl + '/ai';

  constructor(private http: HttpClient) {}

  generateArtisticImage(imageId: string, imageUrl: string, style: string): Observable<GenerateImageResponse> {
    return this.http.post<GenerateImageResponse>(`${this.apiUrl}/generate`, {
      imageId,
      imageUrl,
      style
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Something went wrong';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      errorMessage = error.error?.message || `Error Code: ${error.status}`;
    }

    return throwError(() => new Error(errorMessage));
  }
}

// 2. Створити Component
@Component({
  selector: 'app-ai-style-selector',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <div class="ai-styles">
      <h3>Choose AI Style</h3>

      <div class="styles-grid">
        @for (style of styles; track style.id) {
          <button
            mat-raised-button
            [class.selected]="selectedStyle === style.id"
            (click)="selectStyle(style.id)"
            [disabled]="isGenerating">
            <img [src]="style.preview" [alt]="style.name">
            <span>{{ style.name }}</span>
          </button>
        }
      </div>

      @if (isGenerating) {
        <div class="generating">
          <mat-spinner diameter="40"></mat-spinner>
          <p>Generating artistic image... Please wait</p>
        </div>
      }

      @if (generatedImageUrl) {
        <div class="result">
          <h4>Generated Image:</h4>
          <img [src]="generatedImageUrl" alt="Generated">
          <button mat-button (click)="regenerate()">Try Another Style</button>
          <button mat-raised-button color="primary" (click)="apply()">Apply</button>
        </div>
      }

      @if (errorMessage) {
        <div class="error">
          {{ errorMessage }}
        </div>
      }
    </div>
  `,
  styles: [`
    .ai-styles {
      padding: 20px;
    }

    .styles-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      gap: 16px;
      margin: 20px 0;
    }

    .styles-grid button {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 10px;

      img {
        width: 100px;
        height: 100px;
        object-fit: cover;
        border-radius: 8px;
      }

      &.selected {
        border: 2px solid #3f51b5;
      }
    }

    .generating, .result {
      text-align: center;
      margin: 20px 0;
    }

    .error {
      color: red;
      padding: 10px;
      background: #ffebee;
      border-radius: 4px;
    }
  `]
})
export class AIStyleSelectorComponent {
  @Input() imageId!: string;
  @Input() imageUrl!: string;
  @Output() imageGenerated = new EventEmitter<string>();

  styles = [
    { id: 'watercolor', name: 'Watercolor', preview: '/assets/styles/watercolor.jpg' },
    { id: 'cartoon', name: 'Cartoon', preview: '/assets/styles/cartoon.jpg' },
    { id: 'oil', name: 'Oil Painting', preview: '/assets/styles/oil.jpg' },
    { id: 'sketch', name: 'Sketch', preview: '/assets/styles/sketch.jpg' },
    { id: 'modern', name: 'Modern Art', preview: '/assets/styles/modern.jpg' }
  ];

  selectedStyle: string | null = null;
  isGenerating = false;
  generatedImageUrl: string | null = null;
  errorMessage: string | null = null;

  constructor(private aiService: AIService) {}

  selectStyle(styleId: string) {
    this.selectedStyle = styleId;
    this.generate();
  }

  generate() {
    if (!this.selectedStyle) return;

    this.isGenerating = true;
    this.errorMessage = null;
    this.generatedImageUrl = null;

    this.aiService.generateArtisticImage(this.imageId, this.imageUrl, this.selectedStyle)
      .subscribe({
        next: (response) => {
          this.generatedImageUrl = response.generatedImageUrl;
          this.isGenerating = false;
        },
        error: (error) => {
          this.errorMessage = error.message;
          this.isGenerating = false;
        }
      });
  }

  regenerate() {
    this.generatedImageUrl = null;
    this.selectedStyle = null;
  }

  apply() {
    if (this.generatedImageUrl) {
      this.imageGenerated.emit(this.generatedImageUrl);
    }
  }
}
```

### 4. Testing

**Unit Tests (Backend):**

```csharp
public class ReplicateServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<ReplicateService>> _loggerMock;
    private readonly ReplicateService _service;

    public ReplicateServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<ReplicateService>>();

        _configurationMock.Setup(x => x["Replicate:ApiKey"]).Returns("test-api-key");
        _configurationMock.Setup(x => x["Replicate:ModelVersion"]).Returns("test-version");

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _service = new ReplicateService(httpClient, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateArtisticImageAsync_ValidRequest_ReturnsImageUrl()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var style = "watercolor";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    id = "prediction-123",
                    status = "succeeded",
                    output = "https://example.com/generated.jpg"
                }))
            });

        // Act
        var result = await _service.GenerateArtisticImageAsync(imageUrl, style, CancellationToken.None);

        // Assert
        result.Should().Be("https://example.com/generated.jpg");
    }

    [Fact]
    public async Task GenerateArtisticImageAsync_ApiError_ThrowsException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act & Assert
        await Assert.ThrowsAsync<ExternalServiceException>(
            () => _service.GenerateArtisticImageAsync("url", "style", CancellationToken.None)
        );
    }
}
```

**Unit Tests (Frontend):**

```typescript
describe('AIStyleSelectorComponent', () => {
  let component: AIStyleSelectorComponent;
  let fixture: ComponentFixture<AIStyleSelectorComponent>;
  let aiService: jasmine.SpyObj<AIService>;

  beforeEach(() => {
    const aiServiceSpy = jasmine.createSpyObj('AIService', ['generateArtisticImage']);

    TestBed.configureTestingModule({
      imports: [AIStyleSelectorComponent],
      providers: [
        { provide: AIService, useValue: aiServiceSpy }
      ]
    });

    fixture = TestBed.createComponent(AIStyleSelectorComponent);
    component = fixture.componentInstance;
    aiService = TestBed.inject(AIService) as jasmine.SpyObj<AIService>;

    component.imageId = 'test-id';
    component.imageUrl = 'test-url';
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should generate image when style is selected', () => {
    const mockResponse = { generatedImageUrl: 'generated-url' };
    aiService.generateArtisticImage.and.returnValue(of(mockResponse));

    component.selectStyle('watercolor');

    expect(component.isGenerating).toBe(false);
    expect(component.generatedImageUrl).toBe('generated-url');
    expect(aiService.generateArtisticImage).toHaveBeenCalledWith('test-id', 'test-url', 'watercolor');
  });

  it('should handle error', () => {
    aiService.generateArtisticImage.and.returnValue(throwError(() => new Error('API Error')));

    component.selectStyle('watercolor');

    expect(component.isGenerating).toBe(false);
    expect(component.errorMessage).toBe('API Error');
  });
});
```

### 5. Local Testing

```bash
# Backend
dotnet run --project src/Calendary.Api

# Frontend
cd src/Calendary.Ng
npm start

# Test API з Postman
POST http://localhost:5000/api/v1/ai/generate
{
  "imageId": "guid",
  "imageUrl": "https://example.com/image.jpg",
  "style": "watercolor"
}
```

### 6. Code Review

```bash
git add .
git commit -m "feat(AI): Replicate integration for artistic styles

- Add ReplicateService for API integration
- Implement polling mechanism for async jobs
- Create AIStyleSelectorComponent with 5 style options
- Add error handling and loading states
- Write unit tests (coverage 85%)

Closes US-101"

git push origin feature/US-101-replicate-integration

# Створити Pull Request
```

---

## Code Standards

### C# Coding Style

**Good Practices:**

```csharp
// ✅ Use async/await properly
public async Task<Calendar> GetCalendarAsync(Guid id)
{
    return await _context.Calendars
        .Include(c => c.Images)
        .FirstOrDefaultAsync(c => c.Id == id);
}

// ❌ Don't block async calls
public Calendar GetCalendar(Guid id)
{
    return _context.Calendars.Find(id).Result; // Deadlock risk!
}

// ✅ Use proper exception handling
try
{
    var result = await ExternalApiCall();
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "API call failed");
    throw new ExternalServiceException("Service unavailable", ex);
}

// ❌ Don't swallow exceptions
catch (Exception ex)
{
    // Silent failure - BAD!
}

// ✅ Use meaningful names
var activeCalendars = await _context.Calendars
    .Where(c => c.Status == CalendarStatus.Active)
    .ToListAsync();

// ❌ Avoid cryptic names
var c = await _context.Calendars.Where(x => x.S == 1).ToListAsync();
```

### TypeScript Coding Style

**Good Practices:**

```typescript
// ✅ Use type safety
interface Calendar {
  id: string;
  userId: string;
  templateId: string;
  status: CalendarStatus;
}

function getCalendar(id: string): Observable<Calendar> {
  return this.http.get<Calendar>(`/api/calendars/${id}`);
}

// ❌ Avoid 'any'
function getCalendar(id: any): Observable<any> { // Bad!
  return this.http.get(`/api/calendars/${id}`);
}

// ✅ Use RxJS operators properly
this.calendars$ = this.calendarService.getCalendars().pipe(
  map(calendars => calendars.filter(c => c.status === 'active')),
  catchError(error => {
    this.errorService.handle(error);
    return of([]);
  })
);

// ✅ Unsubscribe properly (or use async pipe)
ngOnInit() {
  this.subscription = this.service.getData().subscribe(data => {
    this.data = data;
  });
}

ngOnDestroy() {
  this.subscription?.unsubscribe();
}

// ✅ Better: use async pipe (auto-unsubscribe)
// Template: {{ calendars$ | async }}
```

---

## Common Tasks

### Додавання нового API endpoint

**Checklist:**
1. [ ] Створити Request/Response DTOs
2. [ ] Створити Command/Query з MediatR
3. [ ] Створити Handler з бізнес-логікою
4. [ ] Додати Validator (FluentValidation)
5. [ ] Створити Controller endpoint
6. [ ] Написати unit tests
7. [ ] Оновити Swagger документацію
8. [ ] Протестувати в Postman

### Додавання нового UI компонента

**Checklist:**
1. [ ] Створити standalone component
2. [ ] Додати типи (interfaces)
3. [ ] Створити service (якщо потрібно)
4. [ ] Додати стилі (SCSS)
5. [ ] Написати unit tests
6. [ ] Додати до потрібного route
7. [ ] Протестувати в браузері

### Database Migration

```bash
# Створити міграцію
dotnet ef migrations add AddAIGeneratedImagesTable -p src/Calendary.Infrastructure -s src/Calendary.Api

# Переглянути SQL
dotnet ef migrations script -p src/Calendary.Infrastructure -s src/Calendary.Api

# Застосувати
dotnet ef database update -p src/Calendary.Infrastructure -s src/Calendary.Api
```

---

## Debugging

### Backend Debugging

**Breakpoints in Visual Studio/Rider:**
- F9 - Set breakpoint
- F5 - Start debugging
- F10 - Step over
- F11 - Step into

**Logging:**
```csharp
_logger.LogDebug("Processing calendar {CalendarId}", calendarId);
_logger.LogInformation("Calendar created successfully");
_logger.LogWarning("AI generation took longer than expected: {Duration}ms", duration);
_logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);
```

### Frontend Debugging

**Chrome DevTools:**
- F12 - Open DevTools
- Console - Logs and errors
- Network - API calls
- Sources - Breakpoints

**Angular DevTools Extension:**
- Component tree
- State inspection
- Performance profiling

---

## Performance Tips

### Backend

```csharp
// ✅ Use AsNoTracking for read-only queries
var calendars = await _context.Calendars
    .AsNoTracking()
    .ToListAsync();

// ✅ Select only needed fields
var calendars = await _context.Calendars
    .Select(c => new CalendarDto
    {
        Id = c.Id,
        Name = c.Name
    })
    .ToListAsync();

// ✅ Use async I/O
await File.WriteAllTextAsync(path, content);

// ❌ Don't block threads
File.WriteAllText(path, content); // Blocking!
```

### Frontend

```typescript
// ✅ Use trackBy in ngFor
<div *ngFor="let calendar of calendars; trackBy: trackByCalendarId">

trackByCalendarId(index: number, calendar: Calendar): string {
  return calendar.id;
}

// ✅ Use OnPush change detection
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})

// ✅ Lazy load modules
const routes: Routes = [
  {
    path: 'editor',
    loadComponent: () => import('./editor/editor.component').then(m => m.EditorComponent)
  }
];
```

---

## Useful Commands

### Git

```bash
# Sync з develop
git fetch origin
git rebase origin/develop

# Squash commits
git rebase -i HEAD~3

# Stash changes
git stash
git stash pop
```

### .NET

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run with watch
dotnet watch run --project src/Calendary.Api
```

### Angular

```bash
# Generate component
ng generate component features/calendar-editor

# Run tests
ng test

# Build for production
ng build --configuration production
```

---

## Learning Resources

### Courses
- Pluralsight: ASP.NET Core Path
- Udemy: Angular - The Complete Guide
- Microsoft Learn: .NET

### Books
- "Clean Code" - Robert Martin
- "Refactoring" - Martin Fowler
- "Design Patterns" - Gang of Four

### Communities
- Stack Overflow
- GitHub Discussions
- .NET Discord
- Angular Discord

---

## Контакти

- **Tech Lead:** [ім'я]
- **Scrum Master:** [ім'я]
- **QA Engineer:** [ім'я]
