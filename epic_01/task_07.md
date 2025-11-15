# Task 07: UI для генерації зображень

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Frontend Dev / Claude Code

## Опис задачі

Створити UI для генерації нових зображень через Replicate API прямо в /editor.

## Проблема

Backend API готовий, потрібен зручний інтерфейс для користувача.

## Що треба зробити

1. **Modal/Panel для генерації**:
   - Кнопка "Згенерувати зображення" в sidebar
   - Відкриває modal з формою
   - Або окрема панель в editor

2. **Форма генерації**:
   - Textarea для prompt (обов'язково)
   - Select для вибору моделі (з списку FluxModel)
   - Width x Height (select: 512x512, 1024x1024, custom)
   - Seed (optional, number, -1 = random)
   - Advanced options (collapse):
     - num_inference_steps (slider 1-50)
     - guidance_scale (slider 0-20)
     - scheduler (select)

3. **Preview generated images**:
   - Gallery згенерованих зображень
   - Click to load in canvas
   - Download button
   - Delete button
   - Metadata (prompt, seed, model, date)

4. **Integration з canvas**:
   - Після генерації - load в canvas автоматично
   - Або кнопка "Use this image"

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/generate-modal/generate-modal.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/generate-modal/generate-modal.component.html`
- `src/Calendary.Ng/src/app/pages/editor/components/image-gallery/image-gallery.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/services/synthesis.service.ts`

## Файли для зміни

- `src/Calendary.Ng/src/app/pages/editor/components/sidebar/sidebar.component.html`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.ts`

## Приклад форми

```typescript
export class GenerateModalComponent {
  generateForm = this.fb.group({
    prompt: ['', [Validators.required, Validators.minLength(10)]],
    modelId: ['', Validators.required],
    width: [1024, [Validators.min(512), Validators.max(2048)]],
    height: [1024, [Validators.min(512), Validators.max(2048)]],
    seed: [-1],
    advancedOptions: this.fb.group({
      numSteps: [28],
      guidanceScale: [3.5],
      scheduler: ['DDIM']
    })
  });

  models: FluxModel[] = [];
  isGenerating = false;
  progress = 0;

  async onSubmit() {
    if (this.generateForm.valid) {
      this.isGenerating = true;

      const result = await this.synthesisService.generate(
        this.generateForm.value
      );

      this.imageGenerated.emit(result.imageUrl);
      this.isGenerating = false;
      this.closeModal();
    }
  }
}
```

## SynthesisService

```typescript
@Injectable({ providedIn: 'root' })
export class SynthesisService {
  private baseUrl = environment.apiUrl + '/synthesis';

  generate(request: GenerateImageRequest): Observable<GenerateImageResponse> {
    return this.http.post<GenerateImageResponse>(
      `${this.baseUrl}/generate`,
      request
    ).pipe(
      catchError(this.handleError)
    );
  }

  getMyImages(): Observable<Synthesis[]> {
    return this.http.get<Synthesis[]>(`${this.baseUrl}/my-images`);
  }
}
```

## Що тестувати

- [ ] Modal відкривається при кліку на "Генерувати"
- [ ] Форма валідується (prompt required, min length)
- [ ] Список моделей завантажується
- [ ] Генерація запускається
- [ ] Loading spinner показується
- [ ] Progress bar оновлюється (якщо є WebSocket)
- [ ] Після генерації - зображення з'являється
- [ ] Зображення можна завантажити в canvas
- [ ] Gallery показує всі згенеровані зображення
- [ ] Metadata відображається
- [ ] Помилки показуються (toast/alert)

## Критерії успіху

- ✅ UI інтуїтивний та зручний
- ✅ Форма валідується коректно
- ✅ Генерація працює
- ✅ Loading states показуються
- ✅ Зображення завантажуються в canvas
- ✅ Gallery функціональна

## Залежності

- [Task 06](./task_06.md) - Інтеграція Replicate API
- [Task 04](./task_04.md) - UI для редактора зображень

## Макет UI

```
+---------------------------------------------+
| Згенерувати зображення                  [X] |
+---------------------------------------------+
| Prompt:                                     |
| [Текстове поле для prompt]                  |
|                                             |
| Модель: [Dropdown з моделями] ▼             |
|                                             |
| Розмір: [1024x1024 ▼]  Seed: [______]      |
|                                             |
| [▼ Додаткові налаштування]                  |
|                                             |
| [Відміна]              [Згенерувати] →      |
+---------------------------------------------+
```

## Примітки

- Додати приклади промптів (placeholder або підказки)
- Зберігати історію промптів (localStorage)
- Auto-save draft prompt

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
