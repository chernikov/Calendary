# Алгоритм роботи з Replicate API

Цей документ описує алгоритми створення моделі та генерації зображень через Replicate API. Ця інформація може бути використана для переносу функціоналу в інший проєкт (наприклад, Telegram бот + Azure Function).

## Конфігурація

### Налаштування (ReplicateSettings)

```json
{
  "ApiKey": "r8_xxx...",
  "Owner": "chernikov",
  "TrainerModel": "ostris/flux-dev-lora-trainer",
  "TrainerVersion": "e440909d3512c31646ee2e0c7d6f6f4923224863a6a10c494606e79fb5844497",
  "WebhookUrl": "https://calendary.com.ua/api/webhook"
}
```

### Заголовки HTTP

Всі запити до Replicate API потребують авторизації:
```
Authorization: Bearer {ApiKey}
```

---

## 1. Алгоритм створення та тренування моделі

### Крок 1.1: Створення моделі в Replicate

**Endpoint:** `POST https://api.replicate.com/v1/models`

**Призначення:** Створює новий приватний контейнер для моделі в обліковому записі Replicate.

**Параметри запиту:**
```json
{
  "owner": "chernikov",
  "name": "chernikov_api_flux_{id}_{random}",
  "description": "Назва моделі",
  "visibility": "private",
  "hardware": "cpu"
}
```

**Відповідь:**
```json
{
  "owner": "chernikov",
  "name": "chernikov_api_flux_123_456",
  "description": "...",
  "visibility": "private",
  "url": "..."
}
```

**Логіка:**
1. Генеруємо унікальне ім'я моделі: `chernikov_api_flux_{fluxModelId}_{randomDigits}`
2. Викликаємо `CreateModelAsync(modelName, description)`
3. Зберігаємо `ReplicateId = "{owner}/{name}"` (наприклад: "chernikov/chernikov_api_flux_123_456")

---

### Крок 1.2: Запуск тренування моделі

**Endpoint:** `POST https://api.replicate.com/v1/models/{TrainerModel}/versions/{TrainerVersion}/trainings`

**Призначення:** Запускає процес тренування LoRA моделі на основі завантажених зображень.

**Параметри запиту:**
```json
{
  "destination": "chernikov/chernikov_api_flux_123_456",
  "input": {
    "steps": 1000,
    "lora_rank": 16,
    "optimizer": "adamw8bit",
    "batch_size": 1,
    "resolution": "512,768,1024",
    "autocaption": true,
    "autocaption_prefix": "a photo of TOK",
    "input_images": "https://calendary.com.ua/uploads/archive_123.zip",
    "trigger_word": "TOK",
    "learning_rate": 0.0004,
    "wandb_project": "flux_train_replicate",
    "wandb_save_interval": 100,
    "wandb_sample_interval": 100,
    "caption_dropout_rate": 0.05,
    "cache_latents_to_disk": false
  },
  "webhook": "https://calendary.com.ua/api/webhook"
}
```

**Відповідь:**
```json
{
  "id": "zz4ibbonubfz7carwiefagzgga",
  "status": "starting",
  "urls": {
    "get": "https://api.replicate.com/v1/predictions/zz4ibbonubfz7carwiefagzgga",
    "cancel": "..."
  },
  ...
}
```

**Логіка:**
1. Підготувати параметри тренування (`TrainModelRequestInput`)
2. Викликати `TrainModelAsync(replicateId, input)`
3. Зберегти в базу даних:
   - `Training.ReplicateId` = response.Id
   - `Training.Status` = "starting"
   - `Training.FluxModelId` = id моделі
4. Оновити статус моделі на "inprocess"

**Статуси тренування:**
- `starting` - тренування ініційоване
- `processing` - тренування виконується
- `succeeded` - успішно завершено
- `failed` - помилка тренування

---

### Крок 1.3: Отримання результату через WebHook

**Endpoint (який викликає Replicate):** `POST https://calendary.com.ua/api/webhook`

**Призначення:** Replicate автоматично надсилає повідомлення після завершення тренування.

**Тіло WebHook запиту:**
```json
{
  "id": "zz4ibbonubfz7carwiefagzgga",
  "status": "succeeded",
  "output": {
    "version": "chernikov/chernikov_api_flux_123_456:abc123def456...",
    "weights": "https://replicate.delivery/..."
  },
  "logs": "...",
  "metrics": {...}
}
```

**Логіка обробки WebHook:**
1. Отримати тіло запиту та десеріалізувати у `WebhookRequest`
2. Знайти `Training` за `replicateId`
3. Оновити `Training.Status` = новий статус
4. Витягти версію моделі: `version = output.version.split(':')[1]`
5. Оновити `Training.Version` та `FluxModel.Version`
6. Оновити `FluxModel.Status` = "processed"
7. **Опціонально:** Запустити генерацію тестових зображень (DefaultJob)

---

### Крок 1.4 (Опціонально): Перевірка статусу тренування вручну

**Endpoint:** `GET https://api.replicate.com/v1/predictions/{replicateId}`

**Призначення:** Ручна перевірка статусу тренування без WebHook.

**Відповідь:**
```json
{
  "id": "zz4ibbonubfz7carwiefagzgga",
  "status": "succeeded",
  "output": {...},
  "logs": "...",
  "metrics": {...}
}
```

**Використання:**
```csharp
var status = await GetTrainingStatusAsync(replicateId);
```

---

## 2. Алгоритм генерації зображень

### Крок 2.1: Відправка запиту на генерацію

**Endpoint:** `POST https://api.replicate.com/v1/predictions`

**Призначення:** Генерує зображення на основі натренованої моделі та текстового промпту.

**Заголовки:**
```
Authorization: Bearer {ApiKey}
Prefer: wait
```

**Параметри запиту:**
```json
{
  "version": "abc123def456...",
  "input": {
    "model": "dev",
    "prompt": "a photo of TOK person at the beach",
    "seed": 12345,
    "lora_scale": 1.0,
    "num_outputs": 1,
    "aspect_ratio": "1:1",
    "output_format": "jpg",
    "guidance_scale": 3.5,
    "output_quality": 90,
    "prompt_strength": 0.8,
    "extra_lora_scale": 1.0,
    "num_inference_steps": 28
  }
}
```

**Пояснення параметрів:**
- `version` - версія натренованої моделі (отримується після тренування)
- `prompt` - текстовий опис зображення (обов'язково містить trigger_word "TOK")
- `seed` - випадкове число для відтворюваності результатів (опціонально)
- `lora_scale` - сила застосування LoRA (0-1)
- `guidance_scale` - наскільки точно слідувати промпту
- `num_inference_steps` - кількість кроків генерації (більше = якісніше, але повільніше)

**Відповідь (з заголовком Prefer: wait):**
```json
{
  "id": "prediction_id_123",
  "status": "succeeded",
  "output": [
    "https://replicate.delivery/pbxt/.../output.jpg"
  ],
  "logs": "Using seed: 12345\n...",
  "metrics": {
    "predict_time": 5.2
  }
}
```

**Логіка:**
1. Підготувати параметри генерації (`GenerateImageInput`)
2. Викликати `GenerateImageAsync(modelVersion, input)`
3. Завдяки заголовку `Prefer: wait`, Replicate повертає готовий результат синхронно
4. Отримати URL зображення з `response.output[0]`

---

### Крок 2.2: Отримання seed з логів

**Призначення:** Витягти реальний seed, який використала модель (якщо не вказували).

**Endpoint:** `GET https://api.replicate.com/v1/predictions/{predictionId}`

**Відповідь містить поле logs:**
```
Using seed: 12345
...
```

**Логіка:**
```csharp
var info = await GeGenerateImageStatusAsync(predictionId);
var seed = ExtractSeedFromLogs(info.Logs); // Regex: "Using seed: (\d+)"
```

---

### Крок 2.3: Завантаження та збереження зображення

**Призначення:** Завантажити згенероване зображення з CDN Replicate та зберегти локально.

**Логіка:**
1. Отримати URL зображення з `output[0]`
2. Завантажити через `HttpClient.GetAsync(imageUrl)`
3. Зберегти як файл: `{Guid.NewGuid()}.jpg`
4. Повернути локальний шлях: `uploads/{filename}.jpg`

**Код:**
```csharp
var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
var fileName = $"{Guid.NewGuid()}.jpg";
var path = Path.Combine("uploads", fileName);
await File.WriteAllBytesAsync(path, imageBytes);
return path;
```

---

## 3. Додаткові операції

### 3.1 Скасування тренування/генерації

**Endpoint:** `POST https://api.replicate.com/v1/predictions/{replicateId}/cancel`

**Використання:**
```csharp
await CancelTrainingAsync(replicateId);
```

---

## 4. Структура даних

### Training (база даних)
```csharp
{
  "Id": 1,
  "FluxModelId": 123,
  "ReplicateId": "zz4ibbonubfz7carwiefagzgga",
  "Status": "succeeded",
  "Version": "abc123def456...",
  "CreatedAt": "2025-01-01T10:00:00Z",
  "CompletedAt": "2025-01-01T10:30:00Z"
}
```

### FluxModel (база даних)
```csharp
{
  "Id": 123,
  "ReplicateId": "chernikov/chernikov_api_flux_123_456",
  "Version": "abc123def456...",
  "Status": "processed",
  "ArchiveUrl": "uploads/archive_123.zip"
}
```

### Synthesis/JobTask (база даних)
```csharp
{
  "Id": 1,
  "TrainingId": 1,
  "ReplicateId": "prediction_id_123",
  "Text": "a photo of TOK person at the beach",
  "Seed": 12345,
  "OutputSeed": 12345,
  "ImageUrl": "uploads/abc-123.jpg",
  "ProcessedImageUrl": "https://replicate.delivery/.../output.jpg",
  "Status": "completed"
}
```

---

## 5. Перенесення на Telegram Bot + Azure Function

### Архітектура

```
┌─────────────────┐
│  Telegram Bot   │
│   (User Input)  │
└────────┬────────┘
         │
         ▼
┌─────────────────────────┐
│   Azure Function        │
│   HTTP Trigger          │
│   - CreateModel         │
│   - TrainModel          │
│   - GenerateImage       │
└───────┬─────────────────┘
        │
        ▼
┌─────────────────────────┐
│   Replicate API         │
│   - api.replicate.com   │
└───────┬─────────────────┘
        │
        ▼
┌─────────────────────────┐
│   Azure Function        │
│   WebHook Endpoint      │
│   - ProcessWebHook      │
└───────┬─────────────────┘
        │
        ▼
┌─────────────────────────┐
│   Azure Storage/Queue   │
│   - Зберігання даних    │
│   - Черга завдань       │
└─────────────────────────┘
        │
        ▼
┌─────────────────────────┐
│   Telegram Bot          │
│   - Відправка результату│
└─────────────────────────┘
```

### Необхідні компоненти

1. **Telegram Bot**
   - Прийом команд від користувача
   - Відправка результатів

2. **Azure Function 1: API для роботи з Replicate**
   - HTTP Trigger
   - Методи:
     - `CreateAndTrainModel(userId, archiveUrl)` → викликає кроки 1.1-1.2
     - `GenerateImage(userId, modelVersion, prompt, seed)` → викликає крок 2.1

3. **Azure Function 2: WebHook Handler**
   - HTTP Trigger
   - Обробка WebHook від Replicate (крок 1.3)
   - Відправка повідомлення користувачу через Telegram

4. **Azure Storage**
   - **Table Storage** або **Cosmos DB**: зберігання статусів тренувань та генерацій
   - **Blob Storage**: зберігання архівів зображень та результатів
   - **Queue Storage**: черга завдань для асинхронної обробки

5. **Application Insights**: логування та моніторинг

### Приклад коду для Azure Function

#### Function: CreateAndTrainModel

```csharp
[FunctionName("CreateAndTrainModel")]
public async Task<IActionResult> CreateAndTrainModel(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
    [Table("Trainings")] IAsyncCollector<TrainingEntity> trainingTable,
    ILogger log)
{
    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    var data = JsonSerializer.Deserialize<CreateModelRequest>(requestBody);
    
    // Крок 1.1: Створення моделі
    var modelName = $"telegram_flux_{data.UserId}_{Random.Shared.Next(100, 999)}";
    var createResponse = await replicateService.CreateModelAsync(modelName, "User model");
    var replicateId = $"{createResponse.Owner}/{createResponse.Name}";
    
    // Крок 1.2: Запуск тренування
    var trainingInput = new TrainModelRequestInput
    {
        InputImages = data.ArchiveUrl,
        TriggerWord = "TOK",
        Steps = 1000,
        // ... інші параметри
    };
    
    var trainingResponse = await replicateService.TrainModelAsync(replicateId, trainingInput);
    
    // Зберегти в Table Storage
    await trainingTable.AddAsync(new TrainingEntity
    {
        PartitionKey = data.UserId,
        RowKey = trainingResponse.Id,
        ReplicateId = trainingResponse.Id,
        Status = "starting",
        ModelId = replicateId
    });
    
    return new OkObjectResult(new { trainingId = trainingResponse.Id });
}
```

#### Function: WebHook Handler

```csharp
[FunctionName("ReplicateWebHook")]
public async Task<IActionResult> WebHook(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
    [Table("Trainings")] CloudTable trainingTable,
    [Queue("telegram-notifications")] IAsyncCollector<TelegramMessage> queue,
    ILogger log)
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    var webhook = JsonSerializer.Deserialize<WebhookRequest>(body);
    
    // Знайти тренування в Table Storage
    var query = TableOperation.Retrieve<TrainingEntity>("", webhook.Id);
    var result = await trainingTable.ExecuteAsync(query);
    var training = (TrainingEntity)result.Result;
    
    if (training != null)
    {
        // Оновити статус
        training.Status = webhook.Status;
        training.Version = GetVersion(webhook.Output.Version);
        await trainingTable.ExecuteAsync(TableOperation.Replace(training));
        
        // Відправити повідомлення в Telegram через чергу
        await queue.AddAsync(new TelegramMessage
        {
            UserId = training.PartitionKey,
            Text = $"Тренування завершено! Версія: {training.Version}"
        });
    }
    
    return new OkResult();
}
```

#### Function: GenerateImage

```csharp
[FunctionName("GenerateImage")]
public async Task<IActionResult> GenerateImage(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
    [Blob("images")] BlobContainerClient blobContainer,
    [Queue("telegram-notifications")] IAsyncCollector<TelegramMessage> queue,
    ILogger log)
{
    var data = JsonSerializer.Deserialize<GenerateRequest>(await req.ReadAsStringAsync());
    
    // Генерація зображення
    var imageInput = new GenerateImageInput
    {
        Prompt = data.Prompt,
        Seed = data.Seed,
        // ... інші параметри
    };
    
    var response = await replicateService.GenerateImageAsync(data.ModelVersion, imageInput);
    
    // Завантажити та зберегти в Blob Storage
    var imageUrl = response.Output[0];
    var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
    var blobName = $"{Guid.NewGuid()}.jpg";
    await blobContainer.UploadBlobAsync(blobName, new BinaryData(imageBytes));
    
    // Отримати публічний URL
    var publicUrl = blobContainer.GetBlobClient(blobName).Uri.ToString();
    
    // Відправити в Telegram
    await queue.AddAsync(new TelegramMessage
    {
        UserId = data.UserId,
        ImageUrl = publicUrl
    });
    
    return new OkObjectResult(new { imageUrl = publicUrl });
}
```

### Налаштування в Azure

1. **Створити Function App**
2. **Налаштувати Application Settings:**
   ```
   ReplicateApiKey=r8_xxx...
   ReplicateOwner=yourname
   ReplicateTrainerModel=ostris/flux-dev-lora-trainer
   ReplicateTrainerVersion=e440909d...
   WebhookUrl=https://yourapp.azurewebsites.net/api/ReplicateWebHook
   TelegramBotToken=123456:ABC-DEF...
   ```

3. **Створити Storage Account**
   - Table: Trainings
   - Blob Container: images, archives
   - Queue: telegram-notifications

4. **Налаштувати Telegram Bot**
   - Обробка команд
   - Відправка результатів з Queue

### Послідовність дій користувача

1. Користувач надсилає фото в Telegram → Bot завантажує в Blob Storage
2. Bot викликає Azure Function `CreateAndTrainModel`
3. Azure Function створює модель та запускає тренування
4. Replicate надсилає WebHook після завершення
5. WebHook Function відправляє повідомлення в чергу
6. Telegram Bot читає з черги та повідомляє користувача
7. Користувач надсилає промпт → Bot викликає `GenerateImage`
8. Azure Function генерує зображення та зберігає в Blob
9. Telegram Bot отримує URL та відправляє фото користувачу

---

## Висновок

Основні API endpoints Replicate:
- **Створення моделі:** `POST /v1/models`
- **Тренування:** `POST /v1/models/{trainer}/versions/{version}/trainings`
- **Генерація:** `POST /v1/predictions` (з заголовком `Prefer: wait`)
- **Статус:** `GET /v1/predictions/{id}`
- **Скасування:** `POST /v1/predictions/{id}/cancel`

Ключові моменти для переносу:
- Використовувати WebHook для асинхронного отримання результатів тренування
- Зберігати стан у базі даних (Table Storage або Cosmos DB)
- Використовувати черги для комунікації між компонентами
- Зберігати файли в Blob Storage
