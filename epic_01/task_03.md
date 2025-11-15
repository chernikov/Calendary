# Task 03: Створити /editor маршрут

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: GPT/Codex

## Опис задачі

Створити новий маршрут `/editor` в Angular додатку для редактора зображень.

## Проблема

Зараз немає окремої сторінки для роботи з редактором зображень. Потрібен новий маршрут.

## Що треба зробити

1. **Створити компонент Editor**:
   ```bash
   ng generate component pages/editor
   ```

2. **Додати маршрут в app.routes.ts**:
   ```typescript
   {
     path: 'editor',
     component: EditorComponent,
     canActivate: [AuthGuard] // тільки для авторизованих
   }
   ```

3. **Створити базову структуру**:
   - Header з навігацією
   - Sidebar з інструментами
   - Main canvas area
   - Footer з контролами

4. **Додати пункт в меню**:
   - Header navigation: "Редактор"
   - Icon для редактора

5. **Guard для авторизації**:
   - Редирект на /login якщо не авторизований

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/editor.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.html`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.scss`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.spec.ts`

## Файли для зміни

- `src/Calendary.Ng/src/app/app.routes.ts`
- `src/Calendary.Ng/src/app/components/header/header.component.html`

## Структура компонента

```typescript
export class EditorComponent implements OnInit {
  // Стан редактора
  currentImage: string | null = null;
  isLoading: boolean = false;

  // Моделі користувача
  userModels: FluxModel[] = [];
  activeModel: FluxModel | null = null;

  ngOnInit() {
    this.loadUserModels();
  }

  loadUserModels() {
    // Завантажити моделі з API
  }
}
```

## Що тестувати

- [ ] /editor відкривається без помилок
- [ ] Редирект на /login якщо не авторизований
- [ ] Пункт меню "Редактор" видимий
- [ ] Клік на меню переходить на /editor
- [ ] Breadcrumbs показує "Головна > Редактор"
- [ ] Базова структура відображається
- [ ] Responsive design працює (mobile, tablet, desktop)

## Критерії успіху

- ✅ /editor доступний для авторизованих користувачів
- ✅ Базова структура HTML/CSS готова
- ✅ Навігація працює
- ✅ AuthGuard блокує неавторизованих

## Залежності

- [Task 01](./task_01.md) - Програма має запускатись

## Макет UI

```
+------------------------------------------+
| Header (Logo, Menu, Profile)            |
+------------------------------------------+
| Sidebar  |  Canvas Area                  |
| Tools    |                               |
|          |  [Зображення тут]             |
|          |                               |
|          |                               |
+------------------------------------------+
| Footer (Controls, Actions)               |
+------------------------------------------+
```

## Примітки

- Використовувати Material UI або Bootstrap для базової стилізації
- Додати loading spinner при переході на сторінку

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
