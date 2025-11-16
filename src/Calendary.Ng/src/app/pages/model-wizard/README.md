# Model Wizard - Новий механізм створення моделі

## Огляд

Model Wizard - це повний рефакторинг процесу створення моделі для генерації картинок календаря. Замінює старий master flow з 9 окремих компонентів на єдиний wizard з правильною архітектурою.

## Переваги нового підходу

### 🎯 Єдиний компонент замість 9
**Старий підхід:**
- `MasterComponent` як контейнер
- 9 вкладених компонентів з окремими HTML/TS/SCSS файлами
- Складна координація між компонентами через `@Output` events
- Дублювання логіки

**Новий підхід:**
- Один `ModelWizardComponent` з Angular Material Stepper
- Всі кроки в одному місці
- Централізований state management через `ModelCreationService`

### 📡 Real-time оновлення через SignalR
**Старий підхід:**
- HTTP polling кожні 2 секунди
- `window.location.reload()` для оновлення стану
- Втрата стану при перезавантаженні

**Новий підхід:**
- SignalR Hub для real-time оновлень
- Автоматичне оновлення прогресу навчання моделі
- Збереження стану в BehaviorSubject

### 🔄 State Management
**Старий підхід:**
- Стан розкиданий між компонентами
- Немає централізованого управління
- Важко відстежувати flow

**Новий підхід:**
- `ModelCreationService` з BehaviorSubject
- Реактивний стан через RxJS
- Observable для підписки на зміни

### 🚨 Обробка помилок
**Старий підхід:**
- Тільки `console.error()`
- Немає retry логіки
- Користувач не отримує фідбек

**Новий підхід:**
- Retry з експоненційною затримкою
- Відображення помилок в UI
- Можливість повторити операцію

## Структура файлів

```
model-wizard/
├── model-wizard.component.ts       # Головний компонент wizard
├── model-wizard.component.html     # Template з Material Stepper
├── model-wizard.component.scss     # Стилі компонента
└── README.md                       # Ця документація
```

## Залежності

### Services
- **ModelCreationService** (`/src/services/model-creation.service.ts`)
  - Управління створенням та життєвим циклом моделі
  - SignalR інтеграція для real-time оновлень
  - State management через BehaviorSubject

### Models
- **FluxModel** (`/src/models/flux-model.ts`) - модель даних
- **ModelStatus** (`/src/models/model-status.enum.ts`) - enum статусів моделі

## Кроки Wizard

### 1. Вибір категорії
- Користувач обирає категорію (людина, тварина, об'єкт)
- Створення FluxModel через API
- Перехід до наступного кроку

### 2. Завантаження фото
- Мінімум 12 фотографій
- Upload через FormData
- Прогрес бар завантаження

### 3. Оплата
- Відображення ціни
- Інтеграція з payment gateway
- Оновлення статусу після оплати

### 4. Навчання моделі
- Real-time прогрес через SignalR
- Анімація процесу
- Можливість закрити вікно (отримання email)

### 5. Перегляд прикладів
- Відображення згенерованих прикладів
- Вибір теми для календаря

### 6. Генерація зображень
- Генерація зображень для календаря
- Прогрес в real-time
- Перехід до редактора

### 7. Завершення
- Повідомлення про готовність
- Кнопка переходу до Editor

## Використання

### Маршрутизація
```typescript
// app.routes.ts
{ path: 'master', component: ModelWizardComponent, canActivate: [UserGuard] }
{ path: 'create-model', component: ModelWizardComponent, canActivate: [UserGuard] }
```

### State Management
```typescript
// Підписка на стан моделі
this.modelCreationService.state$
  .pipe(takeUntil(this.destroy$))
  .subscribe(state => {
    console.log('Status:', state.status);
    console.log('Progress:', state.progress);
    console.log('Error:', state.error);
  });
```

### Створення моделі
```typescript
// Крок 1: Створення
this.modelCreationService.createModel({
  categoryId: 1,
  name: 'Моя модель'
}).subscribe(model => {
  console.log('Model created:', model);
});

// Крок 2: Завантаження фото
this.modelCreationService.uploadPhotos(modelId, photos).subscribe();

// Крок 3: Початок навчання
this.modelCreationService.startTraining(modelId).subscribe();

// Крок 4: Генерація зображень
this.modelCreationService.generateImages(modelId, promptThemeId).subscribe();
```

## SignalR Events

### ModelStatusUpdated
Оновлення статусу моделі в real-time
```typescript
hubConnection.on('ModelStatusUpdated', (model: FluxModel) => {
  // Обробка оновлення
});
```

### ModelTrainingProgress
Прогрес навчання моделі (0-100)
```typescript
hubConnection.on('ModelTrainingProgress', (modelId: number, progress: number) => {
  // Оновлення прогрес бару
});
```

### ModelError
Помилка в процесі створення/навчання
```typescript
hubConnection.on('ModelError', (modelId: number, error: string) => {
  // Відображення помилки користувачу
});
```

## ModelStatus Enum

```typescript
enum ModelStatus {
  Created = 'created',              // Модель створена
  Uploading = 'uploading',          // Завантаження фото
  AwaitingPayment = 'pay_model',    // Очікування оплати
  Preparing = 'prepare',            // Підготовка
  Training = 'inprocess',           // Навчання
  Trained = 'processed',            // Навчання завершено
  ExamplesGenerated = 'exampled',   // Приклади згенеровані
  GeneratingImages = 'image_generating', // Генерація зображень
  ImagesSelected = 'ready_selected', // Зображення вибрані
  DatesAdded = 'dated',             // Дати додані
  Ready = 'ready',                  // Готово
  Failed = 'failed',                // Помилка
  Archived = 'archived'             // Архівовано
}
```

## Міграція зі старого master flow

### Що видалено
- ❌ `MasterComponent` як контейнер для 9 компонентів
- ❌ HTTP polling для оновлень
- ❌ `window.location.reload()` для оновлення стану
- ❌ Розкиданий стан між компонентами

### Що додано
- ✅ Єдиний `ModelWizardComponent` з Material Stepper
- ✅ SignalR для real-time оновлень
- ✅ Централізований `ModelCreationService`
- ✅ Proper error handling з retry логікою
- ✅ Прогрес бари та індикатори завантаження
- ✅ Enum для статусів моделі

### Backwards Compatibility
- Маршрут `/master` продовжує працювати
- Додано новий маршрут `/create-model`
- API endpoints залишилися незмінними
- FluxModel структура сумісна

## Backend Requirements

### API Endpoints
```
POST   /api/flux-model                    - Створення моделі
POST   /api/flux-model/{id}/photos        - Завантаження фото
POST   /api/flux-model/{id}/train         - Початок навчання
POST   /api/flux-model/{id}/generate-examples - Генерація прикладів
POST   /api/flux-model/{id}/generate      - Генерація зображень
GET    /api/flux-model                    - Поточна модель
GET    /api/flux-model/{id}               - Модель за ID
```

### SignalR Hub
Потрібно створити `ModelUpdatesHub` на бекенді:
```csharp
public class ModelUpdatesHub : Hub
{
    public async Task SubscribeToModel(int modelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"model_{modelId}");
    }

    public async Task UnsubscribeFromModel(int modelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"model_{modelId}");
    }
}

// Відправка оновлень
await _hubContext.Clients.Group($"model_{modelId}")
    .SendAsync("ModelStatusUpdated", model);

await _hubContext.Clients.Group($"model_{modelId}")
    .SendAsync("ModelTrainingProgress", modelId, progress);
```

### Hub Registration
```csharp
// Program.cs
app.MapHub<ModelUpdatesHub>("/hubs/model-updates");
```

## TODO / Future Improvements

1. **Photo Upload Component**
   - Перенести існуючий `PhotoUploadComponent` в wizard
   - Додати drag & drop
   - Preview thumbnails

2. **Payment Integration**
   - Інтеграція з payment gateway
   - Обробка успішної/невдалої оплати

3. **Examples Gallery**
   - Компонент для відображення прикладів
   - Вибір теми з preview

4. **Локалізація**
   - i18n для підтримки англійської мови
   - Винести всі тексти в translation files

5. **Tests**
   - Unit tests для `ModelCreationService`
   - Component tests для `ModelWizardComponent`
   - E2E tests для повного flow

6. **Analytics**
   - Відстеження конверсії на кожному кроці
   - Час проведений на кожному кроці
   - Drop-off rate аналіз

## Troubleshooting

### SignalR не підключається
```typescript
// Перевірте URL хабу
private hubConnection = new HubConnectionBuilder()
  .withUrl('/hubs/model-updates') // Має співпадати з backend
  .build();
```

### Модель не створюється
```typescript
// Перевірте retry логіку
retry({
  count: 3,
  delay: (error, retryCount) => timer(Math.pow(2, retryCount - 1) * 1000)
})
```

### Стан не оновлюється
```typescript
// Переконайтеся що підписалися на state$
this.modelCreationService.state$
  .pipe(takeUntil(this.destroy$))
  .subscribe(state => { /* ... */ });
```

## Автори

Створено як рефакторинг старого master flow для покращення архітектури та user experience.

## License

MIT
