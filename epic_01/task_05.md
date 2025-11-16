# Task 05: Додавання зображень до календаря та видалення

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: GPT/Codex
**Виконано**: 16.11.2025

## Опис задачі

Реалізувати простий функціонал додавання згенерованих зображень до календаря та видалення невдалих.

## Проблема

Після генерації зображення потрібна можливість додати його до календаря (для конкретного місяця) або видалити якщо не сподобалось.

## Що треба зробити

1. **Додавання зображення до календаря**:
   - Кнопка "Додати до календаря" при hover на зображення
   - Modal/Dropdown для вибору місяця (1-12)
   - Підтвердження додавання
   - Відображення в calendar preview

2. **Видалення зображення**:
   - Кнопка "Видалити" (іконка смітника)
   - Підтвердження видалення
   - Видалення з gallery та з календаря (якщо додано)

3. **Calendar Preview**:
   - Grid 3x4 або список місяців (Січень - Грудень)
   - Показує яке зображення призначено для кожного місяця
   - Placeholder якщо зображення ще не призначено
   - Click на місяць → select image modal

4. **Управління призначеннями**:
   - Можна змінити зображення для місяця
   - Drag & Drop (опціонально)
   - Clear assignment для місяця

5. **Валідація**:
   - Не дозволити створити календар якщо не всі 12 місяців заповнені
   - Попередження якщо зображення дублюються

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/calendar-preview/calendar-preview.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/month-selector/month-selector.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/services/calendar-builder.service.ts`
- `src/Calendary.Ng/src/app/pages/editor/models/calendar-assignment.model.ts`

## Файли для зміни

- `src/Calendary.Ng/src/app/pages/editor/components/image-gallery/image-gallery.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.html`

## Реалізація

```typescript
// calendar-builder.service.ts
export interface MonthAssignment {
  month: number; // 1-12
  imageId: string;
  imageUrl: string;
}

export class CalendarBuilderService {
  private assignments = new BehaviorSubject<MonthAssignment[]>([]);
  public assignments$ = this.assignments.asObservable();

  assignImageToMonth(month: number, imageId: string, imageUrl: string) {
    const current = this.assignments.value;
    const existing = current.findIndex(a => a.month === month);

    if (existing >= 0) {
      current[existing] = { month, imageId, imageUrl };
    } else {
      current.push({ month, imageId, imageUrl });
    }

    this.assignments.next([...current]);
    localStorage.setItem('calendar-assignments', JSON.stringify(current));
  }

  removeAssignment(month: number) {
    const current = this.assignments.value.filter(a => a.month !== month);
    this.assignments.next(current);
    localStorage.setItem('calendar-assignments', JSON.stringify(current));
  }

  isComplete(): boolean {
    return this.assignments.value.length === 12;
  }

  clear() {
    this.assignments.next([]);
    localStorage.removeItem('calendar-assignments');
  }
}

// image-gallery.component.ts
export class ImageGalleryComponent {
  addToCalendar(image: SynthesisImage) {
    const month = await this.monthSelector.selectMonth();
    if (month) {
      this.calendarBuilder.assignImageToMonth(month, image.id, image.url);
      this.snackBar.open(`Додано до ${this.getMonthName(month)}`, 'OK');
    }
  }

  deleteImage(image: SynthesisImage) {
    const confirmed = await this.confirmDialog.open(
      'Видалити зображення?',
      'Ця дія незворотна'
    );

    if (confirmed) {
      await this.synthesisService.delete(image.id);
      this.images = this.images.filter(i => i.id !== image.id);
    }
  }
}
```

## Що тестувати

- [ ] Кнопка "Додати до календаря" видима
- [ ] Month selector відкривається
- [ ] Можна вибрати місяць (1-12)
- [ ] Зображення призначається для місяця
- [ ] Calendar preview оновлюється
- [ ] Кнопка "Видалити" працює
- [ ] Підтвердження видалення показується
- [ ] Зображення видаляється з gallery та календаря
- [ ] Валідація: не можна створити календар без 12 зображень
- [ ] Попередження при дублюванні зображень
- [ ] Drag & Drop працює (якщо реалізовано)
- [ ] LocalStorage зберігає assignments
- [ ] Після reload - assignments відновлюються

## Критерії успіху

- ✅ Зображення легко додаються до місяців
- ✅ Calendar preview чітко показує призначення
- ✅ Видалення працює без помилок
- ✅ Валідація працює коректно
- ✅ UX інтуїтивний та зручний

## Залежності

- [Task 04](./task_04.md) - UI для редактора зображень

## Макет Calendar Preview

```
+--------------------------------------------------+
| Календар на 2026 рік                             |
+--------------------------------------------------+
| Січень    | Лютий     | Березень | Квітень       |
| [img 1]   | [img 2]   | [empty]  | [img 4]       |
| ✓         | ✓         | ⚠️       | ✓              |
+--------------------------------------------------+
| Травень   | Червень   | Липень   | Серпень       |
| [img 5]   | [empty]   | [img 7]  | [img 8]       |
+--------------------------------------------------+
| Вересень  | Жовтень   | Листопад | Грудень       |
| [img 9]   | [img 10]  | [img 11] | [img 12]      |
+--------------------------------------------------+
| ✅ Готово: 10/12 місяців                         |
+--------------------------------------------------+
```

## Примітки

- Зберігати assignments в localStorage (для draft календаря)
- При створенні Order - відправити всі assignments на backend
- Додати можливість "Clear all" (очистити всі призначення)

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
