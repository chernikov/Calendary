# Task 06: Інтеграція Replicate API

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Gemini

## Опис задачі

Інтегрувати Replicate API для генерації зображень через Flux моделі.

## Проблема

В editor потрібна можливість генерувати нові зображення через AI. Replicate API вже використовується для тренування моделей, потрібно додати генерацію.

## Що треба зробити

1. **Перевірити існуючий ReplicateService**:
   - `src/Calendary.Core/Services/ReplicateService.cs`
   - Чи є метод для генерації зображень?
   - Якщо ні - додати

2. **Метод GenerateImage**:
   ```csharp
   public async Task<string> GenerateImageAsync(
       string modelVersion,
       string prompt,
       int width = 1024,
       int height = 1024,
       int seed = -1
   )
   {
       // Виклик Replicate API
       // Повертає URL згенерованого зображення
   }
   ```

3. **Додати endpoint в API**:
   - `POST /api/synthesis/generate`
   - Body: `{ prompt, modelId, width, height, seed }`
   - Response: `{ imageUrl, seed, status }`

4. **Polling для статусу**:
   - Replicate API асинхронний
   - Потрібно poll статус (starting → processing → succeeded)
   - WebSocket або SignalR для real-time updates (опціонально)

5. **Обробка помилок**:
   - Timeout (30 секунд)
   - API errors (5xx)
   - Retry logic (Polly)
   - Fallback повідомлення

## Файли для зміни

- `src/Calendary.Core/Services/ReplicateService.cs`
- `src/Calendary.Api/Controllers/SynthesisController.cs` (якщо існує)
- `src/Calendary.Api/Controllers/ImageController.cs` (або створити новий)

## Файли для створення

- `src/Calendary.Api/Dtos/GenerateImageRequest.dto.cs`
- `src/Calendary.Api/Dtos/GenerateImageResponse.dto.cs`

## Приклад коду

```csharp
// ReplicateService.cs
public async Task<GenerateImageResponse> GenerateImageAsync(GenerateImageRequest request)
{
    var input = new
    {
        prompt = request.Prompt,
        width = request.Width,
        height = request.Height,
        num_outputs = 1,
        num_inference_steps = 28,
        guidance_scale = 3.5,
        seed = request.Seed > 0 ? request.Seed : new Random().Next()
    };

    var prediction = await _httpClient.PostAsJsonAsync(
        $"https://api.replicate.com/v1/models/{request.ModelVersion}/predictions",
        new { input }
    );

    var predictionId = prediction.Id;

    // Poll for completion
    while (true)
    {
        var status = await GetPredictionStatus(predictionId);

        if (status.Status == "succeeded")
        {
            return new GenerateImageResponse
            {
                ImageUrl = status.Output[0],
                Seed = status.Input.Seed,
                Status = "completed"
            };
        }

        if (status.Status == "failed")
        {
            throw new Exception(status.Error);
        }

        await Task.Delay(1000);
    }
}
```

## API Endpoint

```csharp
[HttpPost("generate")]
public async Task<IActionResult> GenerateImage([FromBody] GenerateImageRequest request)
{
    var userId = User.GetUserId();
    var user = await _userService.GetByIdAsync(userId);

    // Перевірка балансу (якщо платна генерація)
    // ...

    var result = await _replicateService.GenerateImageAsync(request);

    // Зберегти в БД
    var synthesis = new Synthesis
    {
        UserId = userId,
        Prompt = request.Prompt,
        ImageUrl = result.ImageUrl,
        OutputSeed = result.Seed,
        CreatedAt = DateTime.UtcNow
    };

    await _synthesisRepository.AddAsync(synthesis);

    return Ok(result);
}
```

## Що тестувати

- [ ] POST /api/synthesis/generate повертає 200
- [ ] Генерація завершується успішно
- [ ] imageUrl валідний та доступний
- [ ] seed повертається
- [ ] Timeout після 30 секунд
- [ ] Retry при 5xx errors
- [ ] Помилка при неправильному modelId
- [ ] Помилка при пустому prompt
- [ ] Rate limiting працює (якщо є)
- [ ] Логування всіх запитів

## Критерії успіху

- ✅ API endpoint працює
- ✅ Генерація зображення завершується <30 сек
- ✅ imageUrl доступний та завантажується
- ✅ Помилки обробляються коректно
- ✅ Retry logic працює

## Залежності

- Replicate API key в appsettings.json
- Існуючий ReplicateService
- Flux моделі вже натреновані

## Environment Variables

```json
{
  "ReplicateSettings": {
    "ApiKey": "r8_...",
    "BaseUrl": "https://api.replicate.com/v1",
    "Timeout": 30000,
    "MaxRetries": 3
  }
}
```

## Примітки

- Використовувати існуючий код з TrainingService як референс
- Додати Polly для retry logic
- WebSocket для real-time progress (опціонально, але рекомендовано)

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
