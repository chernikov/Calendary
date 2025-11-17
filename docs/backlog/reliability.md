# Backlog: Reliability & Error Handling

## P1-02: Обробка помилок Replicate API з retry logic

**Джерело**: TASKS.md (Task 2)
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години

### Проблема
При збоях Replicate API (timeout, 5xx) генерація падає без retry.

### Що треба зробити
1. Додати Polly library
2. Exponential backoff
3. Fallback mechanism (queue)
4. Логування retry спроб
5. UI показує "Retrying..." замість "Failed"

### Файли
- `src/Calendary.Core/Services/ReplicateService.cs`
- `src/Calendary.Core/Calendary.Core.csproj` (Polly NuGet)
- `src/Calendary.Consumer/Consumers/*`

### Критерії успіху
- 90%+ тренувань з тимчасовими збоями завершуються успішно

---

## P1-03: Валідація завантажених фото

**Джерело**: TASKS.md (Task 3)
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 2-3 години

### Проблема
Користувачі завантажують неякісні фото → погані результати тренування.

### Що треба зробити
1. Формат: JPG, PNG, WebP only
2. Розмір: 512x512px - 4096x4096px
3. Файл: max 10MB
4. Кількість: 10-30 фото
5. Optional: NSFW detection

### Файли
- `src/Calendary.Api/Controllers/FluxModelController.cs`
- `src/Calendary.Core/Services/ImageValidationService.cs` (новий)
- `src/Calendary.Ng/src/app/components/` (UI помилок)

---

## P2-07: Rate limiting для публічних API

**Джерело**: TASKS.md (Task 7)
**Пріоритет**: P2 (Середній)
**Складність**: Низька
**Час**: 1-2 години

### Що треба зробити
1. AspNetCoreRateLimit NuGet
2. Ліміти:
   - `/api/auth/register`: 5 req/hour per IP
   - `/api/flux-models/train`: 3 req/day per user
   - `/api/synthesis/generate`: 100 req/hour per user
3. 429 Too Many Requests

### Файли
- `src/Calendary.Api/Program.cs`
- `src/Calendary.Api/appsettings.json`

---

## BUG-1: WebHook від MonoPay іноді не спрацьовує

**Джерело**: TASKS.md (Bugfix 1)
**Пріоритет**: P1 (Високий)
**Складність**: Середня

### Проблема
Після оплати замовлення статус не оновлюється автоматично.

### Що перевірити
1. WebhookUrl в MonoPay
2. Endpoint обробка
3. Логування всіх webhooks
4. Retry mechanism

### Файли
- `src/Calendary.Api/Controllers/WebhookController.cs`

---

## BUG-2: Seed не зберігається коректно

**Джерело**: TASKS.md (Bugfix 2)
**Пріоритет**: P2 (Середній)
**Складність**: Низька

### Проблема
`OutputSeed` в `Synthesis` іноді null.

### Що зробити
1. Перевірити парсинг seed
2. Fallback: використати input seed
3. Валідація перед save

### Файли
- `src/Calendary.Core/Services/ReplicateService.cs`

---

**Створено**: 2025-11-15
