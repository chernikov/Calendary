# Task 23: UI для вибору пресетів свят

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: GPT/Codex

## Опис задачі

Створити UI для перегляду та вибору пресетів свят в редакторі календаря. Користувач може вибрати один або кілька пресетів (наприклад, українські державні + православні церковні) для додавання свят до свого календаря.

## Що треба зробити

1. **Holiday Presets Selector Component**:
   - Список доступних пресетів свят
   - Групування за типами (Державні, Релігійні, Міжнародні)
   - Можливість вибрати кілька пресетів одночасно
   - Preview: список свят у пресеті
   - Підтримка локалізації UA/EN

2. **Preset Card UI**:
   ```html
   <div class="holiday-preset-card">
     <div class="preset-header">
       <mat-checkbox [(ngModel)]="preset.selected">
         <h3>{{ preset.name }}</h3>
       </mat-checkbox>
       <span class="preset-type">{{ preset.type }}</span>
     </div>
     <p class="preset-description">{{ preset.description }}</p>
     <div class="holiday-count">
       <mat-icon>event</mat-icon>
       <span>{{ preset.holidayCount }} свят</span>
     </div>
     <button mat-button (click)="previewHolidays(preset)">
       Переглянути свята
     </button>
   </div>
   ```

3. **Presets Gallery Layout**:
   - **Фільтри**:
     * За типом: Державні / Релігійні / Міжнародні / Всі
     * За регіоном: Україна / США / Великобританія / Інше
   - **Grid з карточками пресетів**:
     * 3 колонки на desktop
     * 2 колонки на tablet
     * 1 колонка на mobile
   - **Вибрані пресети (Sticky footer)**:
     * Показує кількість вибраних пресетів
     * Кнопка "Застосувати (N пресетів)"
     * Кнопка "Скинути вибір"

4. **Preview Modal для свят**:
   ```typescript
   interface HolidayPreview {
     month: number;
     day: number;
     name: string;
     isMovable: boolean;
   }
   ```
   - Календарна сітка на рік з відміченими святами
   - Список свят з датами
   - Підсвітка рухомих свят

5. **Apply Confirmation Modal**:
   - Список вибраних пресетів
   - Загальна кількість свят, що буде додано
   - Попередження про дублювання (якщо свято вже є)
   - Опція "Замінити існуючі" або "Додати тільки нові"
   - Підтвердження

## API Integration

```typescript
// preset.service.ts
export interface HolidayPreset {
  id: number;
  code: string;
  type: 'State' | 'Religious' | 'International';
  name: string;
  description: string;
  holidayCount: number;
  holidays: Holiday[];
}

export interface Holiday {
  id: number;
  month: number;
  day: number;
  name: string;
  isMovable: boolean;
}

@Injectable()
export class HolidayPresetService {
  getPresets(language: string): Observable<HolidayPreset[]> {
    return this.http.get<HolidayPreset[]>(
      `/api/holiday-presets?lang=${language}`
    );
  }

  getPresetHolidays(code: string, year: number): Observable<Holiday[]> {
    return this.http.get<Holiday[]>(
      `/api/holiday-presets/${code}/holidays?year=${year}`
    );
  }

  applyPresets(calendarId: number, presetCodes: string[]): Observable<void> {
    return this.http.post<void>(
      `/api/calendars/${calendarId}/apply-presets`,
      { presetCodes }
    );
  }
}
```

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/holiday-presets-gallery/holiday-presets-gallery.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/holiday-presets-gallery/holiday-presets-gallery.component.html`
- `src/Calendary.Ng/src/app/pages/editor/components/holiday-presets-gallery/holiday-presets-gallery.component.scss`
- `src/Calendary.Ng/src/app/pages/editor/components/holiday-preview-modal/holiday-preview-modal.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/services/holiday-preset.service.ts`
- `src/Calendary.Ng/src/app/pages/editor/models/holiday-preset.model.ts`

## Що тестувати

- [ ] Gallery завантажується з усіма пресетами
- [ ] Фільтрація за типом та регіоном працює
- [ ] Можливість вибрати кілька пресетів
- [ ] Preview показує всі свята пресету
- [ ] Рухомі свята відображаються з правильними датами для вибраного року
- [ ] Apply confirmation показує коректну інформацію
- [ ] Локалізація UA/EN працює для всіх назв
- [ ] Responsive layout на різних пристроях
- [ ] Loading states показуються під час завантаження

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-16
