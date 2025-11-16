# Task 04: UI для редактора зображень

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 4-6 годин
**Відповідальний AI**: Claude
**Виконано**: 16.11.2025

## Опис задачі

Створити повноцінний UI для редактора зображень з інструментами та контролами.

## Проблема

Після створення маршруту `/editor` потрібен робочий інтерфейс для редагування зображень.

## Що треба зробити

1. **Sidebar з інструментами**:
   - 🎨 Генерація нового зображення
   - 📁 Завантаження з файлу
   - ✂️ Обрізка (Crop)
   - 🔄 Поворот (Rotate)
   - 📏 Зміна розміру (Resize)
   - 🎭 Фільтри (Brightness, Contrast, Saturation)
   - 💾 Збереження
   - 📤 Експорт

2. **Canvas Area**:
   - Відображення поточного зображення
   - Grid для позиціонування
   - Rulers (лінійки)
   - Zoom controls (10% - 400%)
   - Pan/Scroll для навігації

3. **Properties Panel**:
   - Розмір зображення (ширина x висота)
   - Формат (JPG, PNG, WebP)
   - Якість (1-100%)
   - Metadata (EXIF)

4. **History Panel**:
   - Список дій (History)
   - Undo/Redo кнопки
   - Ctrl+Z / Ctrl+Y shortcuts

5. **Bottom Toolbar**:
   - Zoom slider
   - Fit to screen
   - Actual size
   - Grid on/off
   - Rulers on/off

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/sidebar/sidebar.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/canvas/canvas.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/properties/properties.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/history/history.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/components/toolbar/toolbar.component.ts`

## Файли для зміни

- `src/Calendary.Ng/src/app/pages/editor/editor.component.html`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.scss`
- `src/Calendary.Ng/src/app/pages/editor/editor.component.ts`

## Бібліотеки для використання

- **Fabric.js** або **Konva.js** - для canvas маніпуляцій
- **ngx-image-cropper** - для обрізки зображень
- **Angular Material** - для UI компонентів

```bash
npm install fabric ngx-image-cropper
```

## Структура стану

```typescript
interface EditorState {
  currentImage: HTMLImageElement | null;
  history: EditorAction[];
  historyIndex: number;
  zoom: number;
  gridEnabled: boolean;
  rulersEnabled: boolean;
  selectedTool: EditorTool;
  isDirty: boolean; // чи є незбережені зміни
}

type EditorTool =
  | 'select'
  | 'crop'
  | 'rotate'
  | 'resize'
  | 'filter'
  | 'text'
  | 'draw';

interface EditorAction {
  type: string;
  timestamp: Date;
  data: any;
}
```

## Що тестувати

- [ ] Sidebar відображається з всіма інструментами
- [ ] Canvas відображає зображення
- [ ] Zoom працює (slider + кнопки)
- [ ] Grid показується/ховається
- [ ] Rulers показуються/ховаються
- [ ] Properties показує інформацію про зображення
- [ ] History відображає список дій
- [ ] Undo/Redo працює
- [ ] Keyboard shortcuts працюють (Ctrl+Z, Ctrl+Y)
- [ ] Responsive layout (sidebar collapse на mobile)
- [ ] Попередження про незбережені зміни

## Критерії успіху

- ✅ Всі панелі відображаються коректно
- ✅ Canvas responsive та масштабується
- ✅ Інструменти доступні та кликабельні
- ✅ History зберігає дії
- ✅ Undo/Redo працює
- ✅ UI інтуїтивний та зручний

## Залежності

- [Task 03](./task_03.md) - Створити /editor маршрут

## Макет детальніше

```
+------------------------------------------------------------------+
| Header                                                           |
+--------+-----------------------------------------------+---------+
| TOOLS  | CANVAS                                        | PROPS   |
|        |                                               |         |
| [Gen]  | +------------------------------------------+  | Size:   |
| [Load] | |                                          |  | 1024x   |
| [Crop] | |                                          |  | 1024    |
| [Rot]  | |      [IMAGE HERE]                        |  |         |
| [Size] | |                                          |  | Format: |
| [Filt] | |                                          |  | PNG     |
| [Save] | +------------------------------------------+  |         |
|        |                                               | Quality:|
+--------+-----------------------------------------------+ 95%     |
| History                    | Zoom: [====|====] 100%   |         |
| 1. Crop                    | [Fit] [Actual] [Grid]    |         |
| 2. Rotate 90°              |                           |         |
+----------------------------+---------------------------+---------+
```

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
