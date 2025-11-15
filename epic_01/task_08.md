# Task 08: Відображення прогресу генерації

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P3 (Низький - генерація до 10 сек, не критично)
**Складність**: Середня
**Час**: 2-3 години
**Відповідальний AI**: Full Stack / Claude Code

## Опис задачі

Додати real-time відображення прогресу генерації зображення (WebSocket або SignalR).

## Проблема

Генерація зображення займає 10-30 секунд. Користувач не бачить прогрес і може подумати що щось зламалось.

## Що треба зробити

1. **SignalR Hub на Backend**:
   ```csharp
   public class ImageGenerationHub : Hub
   {
       public async Task JoinGenerationGroup(string generationId)
       {
           await Groups.AddToGroupAsync(Context.ConnectionId, generationId);
       }
   }
   ```

2. **Відправка прогресу**:
   - Starting: 0%
   - Processing: 10% → 90% (approximate)
   - Finalizing: 95%
   - Completed: 100%

3. **Frontend SignalR client**:
   ```typescript
   import * as signalR from '@microsoft/signalr';

   const connection = new signalR.HubConnectionBuilder()
     .withUrl('/hubs/image-generation')
     .build();

   connection.on('ProgressUpdate', (progress) => {
     this.progress = progress;
   });
   ```

4. **Progress UI**:
   - Progress bar (0-100%)
   - Status text ("Starting...", "Processing...", "Done!")
   - Estimated time remaining
   - Cancel button (optional)

## Файли для створення

- `src/Calendary.Api/Hubs/ImageGenerationHub.cs`
- `src/Calendary.Ng/src/app/pages/editor/services/signalr.service.ts`

## Файли для зміни

- `src/Calendary.Api/Program.cs` (додати SignalR)
- `src/Calendary.Core/Services/ReplicateService.cs` (відправляти прогрес)
- `src/Calendary.Ng/src/app/pages/editor/components/generate-modal/generate-modal.component.ts`

## Backend реалізація

```csharp
// Program.cs
builder.Services.AddSignalR();

app.MapHub<ImageGenerationHub>("/hubs/image-generation");

// ReplicateService.cs
public async Task<string> GenerateImageAsync(string generationId, ...)
{
    await _hubContext.Clients.Group(generationId)
        .SendAsync("ProgressUpdate", new { progress = 0, status = "Starting..." });

    var prediction = await CreatePrediction(...);

    while (true)
    {
        var status = await GetPredictionStatus(prediction.Id);

        var progress = CalculateProgress(status);
        await _hubContext.Clients.Group(generationId)
            .SendAsync("ProgressUpdate", new { progress, status: status.Status });

        if (status.Status == "succeeded") break;

        await Task.Delay(1000);
    }

    await _hubContext.Clients.Group(generationId)
        .SendAsync("ProgressUpdate", new { progress = 100, status = "Completed!" });

    return status.Output[0];
}
```

## Frontend реалізація

```typescript
export class SignalRService {
  private connection: signalR.HubConnection;
  public progress$ = new BehaviorSubject<ProgressUpdate | null>(null);

  async connect() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/image-generation')
      .withAutomaticReconnect()
      .build();

    this.connection.on('ProgressUpdate', (update: ProgressUpdate) => {
      this.progress$.next(update);
    });

    await this.connection.start();
  }

  async joinGenerationGroup(generationId: string) {
    await this.connection.invoke('JoinGenerationGroup', generationId);
  }
}

// У компоненті
export class GenerateModalComponent implements OnInit {
  progress$: Observable<ProgressUpdate>;

  ngOnInit() {
    this.progress$ = this.signalRService.progress$;
  }

  async generate() {
    const generationId = uuidv4();
    await this.signalRService.joinGenerationGroup(generationId);

    this.synthesisService.generate({ ...this.form.value, generationId })
      .subscribe();
  }
}
```

## UI Progress bar

```html
<div *ngIf="(progress$ | async) as progress" class="progress-container">
  <mat-progress-bar
    mode="determinate"
    [value]="progress.progress">
  </mat-progress-bar>

  <div class="progress-info">
    <span>{{ progress.status }}</span>
    <span>{{ progress.progress }}%</span>
  </div>

  <div *ngIf="progress.estimatedTime" class="eta">
    Estimated time: {{ progress.estimatedTime }}s
  </div>
</div>
```

## Що тестувати

- [ ] SignalR connection встановлюється
- [ ] Progress updates приходять
- [ ] Progress bar оновлюється плавно
- [ ] Status text змінюється (Starting → Processing → Done)
- [ ] При помилці - показує error status
- [ ] Reconnect працює якщо connection lost
- [ ] Multiple generations не conflict
- [ ] Progress сбрасывается після completion

## Критерії успіху

- ✅ Progress показується в real-time
- ✅ Latency <500ms для updates
- ✅ Reconnect працює автоматично
- ✅ UI responsive та не лагає
- ✅ Estimated time точний (±20%)

## Залежності

- [Task 06](./task_06.md) - Інтеграція Replicate API
- [Task 07](./task_07.md) - UI для генерації зображень

## Alternative: Polling замість SignalR

Якщо SignalR складно - можна використати polling:

```typescript
pollStatus(generationId: string) {
  return interval(1000).pipe(
    switchMap(() => this.http.get(`/api/synthesis/status/${generationId}`)),
    takeWhile(status => status.status !== 'completed'),
    finalize(() => console.log('Generation completed'))
  );
}
```

## Примітки

- SignalR краще для UX (instant updates)
- Polling простіше реалізувати
- Для MVP можна почати з polling, потім додати SignalR

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
