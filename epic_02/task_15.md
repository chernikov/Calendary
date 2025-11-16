# Task 15: Undo/Redo та збереження стану

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: ⚠️ ЧАСТКОВО COMPLETED
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Claude
**Залежить від**: Task 12, 13, 14
**Виконано**: 2025-11-16 (localStorage auto-save ✅, undo/redo ❌, backend save ❌)

## Опис задачі

Імплементувати history stack для undo/redo операцій, auto-save функціонал, збереження canvas state в localStorage та на backend.

## Проблема

Користувачі повинні мати можливість відміняти та повторювати дії, а також не втрачати свою роботу при закритті браузера.

## Що треба зробити

1. **Імплементувати History Stack**
   - Зберігати історію змін canvas
   - Максимум 50 кроків історії
   - Push state при кожній зміні (debounced)
   - Undo/Redo commands

2. **Додати Undo/Redo кнопки**
   - В EditorToolbar
   - Keyboard shortcuts (Ctrl+Z, Ctrl+Shift+Z)
   - Disabled state коли неможливо undo/redo
   - Показувати кількість доступних кроків

3. **Auto-save в localStorage**
   - Зберігати canvas state кожні 10 секунд
   - При завантаженні відновлювати з localStorage
   - Показувати "Automatically saved" індикатор

4. **Manual save на backend**
   - Кнопка "Зберегти" в toolbar
   - PUT /api/calendars/{id}
   - Зберігати canvas JSON та preview image
   - Показувати saving state

5. **Canvas State Serialization**
   - Serialize Fabric.js canvas to JSON
   - Deserialize JSON to canvas
   - Зберігати всі objects з properties
   - Validate JSON перед restore

6. **Preview Image Generation**
   - Generate preview з canvas
   - Canvas.toDataURL()
   - Upload як base64 або file
   - Показувати в списку календарів

## Файли для створення/модифікації

- `src/hooks/useCanvasHistory.ts`
- `src/hooks/useAutoSave.ts`
- `src/components/features/editor/EditorToolbar.tsx` - додати undo/redo
- `src/components/features/editor/SaveButton.tsx`
- `src/services/calendarService.ts`
- `src/components/features/editor/CanvasEditor.tsx` - інтеграція

## Критерії успіху

- [ ] Undo працює (кнопка та Ctrl+Z)
- [ ] Redo працює (кнопка та Ctrl+Shift+Z)
- [ ] Auto-save в localStorage працює
- [ ] При перезавантаженні сторінки відновлюється canvas
- [ ] "Зберегти" кнопка відправляє на backend
- [ ] Preview image генерується автоматично
- [ ] Saving state відображається користувачу

## Залежності

- Task 12: Canvas повинен бути готовий
- Task 13, 14: Всі інструменти повинні працювати

## Блокується наступні задачі

- Task 16: Preview потребує збереженого canvas state

## Технічні деталі

### src/hooks/useCanvasHistory.ts
```typescript
import { useState, useCallback } from 'react'

interface HistoryState {
  canvasJSON: string
}

export function useCanvasHistory(maxHistory = 50) {
  const [history, setHistory] = useState<HistoryState[]>([])
  const [currentIndex, setCurrentIndex] = useState(-1)

  const canUndo = currentIndex > 0
  const canRedo = currentIndex < history.length - 1

  const pushState = useCallback((canvasJSON: string) => {
    setHistory((prev) => {
      // Remove any states after current index (for redo)
      const newHistory = prev.slice(0, currentIndex + 1)

      // Add new state
      newHistory.push({ canvasJSON })

      // Limit history size
      if (newHistory.length > maxHistory) {
        newHistory.shift()
        return newHistory
      }

      return newHistory
    })

    setCurrentIndex((prev) => Math.min(prev + 1, maxHistory - 1))
  }, [currentIndex, maxHistory])

  const undo = useCallback(() => {
    if (!canUndo) return null

    setCurrentIndex((prev) => prev - 1)
    return history[currentIndex - 1]
  }, [canUndo, currentIndex, history])

  const redo = useCallback(() => {
    if (!canRedo) return null

    setCurrentIndex((prev) => prev + 1)
    return history[currentIndex + 1]
  }, [canRedo, currentIndex, history])

  return {
    pushState,
    undo,
    redo,
    canUndo,
    canRedo,
  }
}
```

### src/hooks/useAutoSave.ts
```typescript
import { useEffect, useRef } from 'react'
import { useDebounce } from '@/hooks/useDebounce'

export function useAutoSave(
  calendarId: string,
  getCanvasJSON: () => string,
  interval = 10000 // 10 seconds
) {
  const timeoutRef = useRef<NodeJS.Timeout>()

  useEffect(() => {
    const save = () => {
      const canvasJSON = getCanvasJSON()
      localStorage.setItem(`calendar-${calendarId}`, canvasJSON)
      console.log('Auto-saved to localStorage')
    }

    timeoutRef.current = setInterval(save, interval)

    return () => {
      if (timeoutRef.current) {
        clearInterval(timeoutRef.current)
      }
    }
  }, [calendarId, getCanvasJSON, interval])
}
```

### Undo/Redo Integration in CanvasEditor
```typescript
'use client'

import { useEffect } from 'react'
import { useCanvasHistory } from '@/hooks/useCanvasHistory'

export default function CanvasEditor() {
  const [fabricCanvas, setFabricCanvas] = useState<fabric.Canvas | null>(null)
  const { pushState, undo, redo, canUndo, canRedo } = useCanvasHistory()

  useEffect(() => {
    if (!fabricCanvas) return

    // Push state on object modifications (debounced)
    const handleModified = debounce(() => {
      const json = JSON.stringify(fabricCanvas.toJSON())
      pushState(json)
    }, 300)

    fabricCanvas.on('object:modified', handleModified)
    fabricCanvas.on('object:added', handleModified)
    fabricCanvas.on('object:removed', handleModified)

    return () => {
      fabricCanvas.off('object:modified', handleModified)
      fabricCanvas.off('object:added', handleModified)
      fabricCanvas.off('object:removed', handleModified)
    }
  }, [fabricCanvas, pushState])

  const handleUndo = () => {
    const state = undo()
    if (state && fabricCanvas) {
      fabricCanvas.loadFromJSON(state.canvasJSON, () => {
        fabricCanvas.requestRenderAll()
      })
    }
  }

  const handleRedo = () => {
    const state = redo()
    if (state && fabricCanvas) {
      fabricCanvas.loadFromJSON(state.canvasJSON, () => {
        fabricCanvas.requestRenderAll()
      })
    }
  }

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.ctrlKey && e.key === 'z' && !e.shiftKey) {
        e.preventDefault()
        handleUndo()
      } else if (e.ctrlKey && e.shiftKey && e.key === 'z') {
        e.preventDefault()
        handleRedo()
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [handleUndo, handleRedo])

  return (
    <EditorToolbar
      onUndo={handleUndo}
      onRedo={handleRedo}
      canUndo={canUndo}
      canRedo={canRedo}
    />
  )
}
```

### Save to Backend
```typescript
const handleSave = async () => {
  if (!fabricCanvas) return

  try {
    setSaving(true)

    // Get canvas JSON
    const designData = JSON.stringify(fabricCanvas.toJSON())

    // Generate preview image
    const previewDataURL = fabricCanvas.toDataURL({
      format: 'png',
      quality: 0.8,
      multiplier: 0.5, // Half size for preview
    })

    // Upload preview image
    const blob = await fetch(previewDataURL).then(r => r.blob())
    const previewFile = new File([blob], 'preview.png', { type: 'image/png' })
    const { url: previewImageUrl } = await fileService.upload(previewFile)

    // Save calendar
    await calendarService.update(calendarId, {
      designData,
      previewImageUrl,
      status: 'Draft',
    })

    toast.success('Календар збережено!')
  } catch (error) {
    console.error('Save failed:', error)
    toast.error('Помилка збереження')
  } finally {
    setSaving(false)
  }
}
```

## Примітки

- Debounce важливий для performance (не зберігати кожен pixel)
- localStorage має ліміт ~5MB (достатньо для canvas state)
- Preview image покращує UX в списку календарів
- Keyboard shortcuts покращують workflow

## Чому Claude?

Складна state management задача:
- History stack implementation
- Canvas serialization/deserialization
- Auto-save timing та debouncing
- Integration між localStorage та backend

---

## Фактична реалізація в Angular

**Важливо**: Ця задача стосується **збереження стану календаря** (assignments), а не image editing операцій.

### Реалізовано ✅:

#### 1. Auto-save в localStorage

**CalendarBuilderService** (calendar-builder.service.ts:9-10, 76-90):

```typescript
export class CalendarBuilderService {
  private readonly storageKey = 'calendar-assignments';
  private readonly assignmentsSubject = new BehaviorSubject<MonthAssignment[]>(
    this.loadFromStorage() // Автоматичне завантаження при старті
  );

  // Автоматичне збереження при кожній зміні
  private setAssignments(assignments: MonthAssignment[]): void {
    this.assignmentsSubject.next(assignments);
    this.persist(assignments); // ← Auto-save
  }

  private persist(assignments: MonthAssignment[]): void {
    if (typeof localStorage === 'undefined') return;

    try {
      localStorage.setItem(this.storageKey, JSON.stringify(assignments));
    } catch {
      // Silently fail storage errors
    }
  }

  private loadFromStorage(): MonthAssignment[] {
    if (typeof localStorage === 'undefined') return [];

    try {
      const saved = localStorage.getItem(this.storageKey);
      return saved ? JSON.parse(saved) : [];
    } catch {
      return [];
    }
  }
}
```

**Як працює:**
- ✅ При `assignImageToMonth()` → auto-save в localStorage
- ✅ При `removeAssignment()` → auto-save в localStorage
- ✅ При `clear()` → auto-save в localStorage
- ✅ При завантаженні сторінки → auto-restore з localStorage

#### 2. Reactive State Management

**BehaviorSubject для reactive updates** (calendar-builder.service.ts:10-11):

```typescript
private readonly assignmentsSubject = new BehaviorSubject<MonthAssignment[]>(
  this.loadFromStorage()
);
readonly assignments$ = this.assignmentsSubject.asObservable();
```

**EditorComponent підписується на зміни** (editor.component.ts:397-404):

```typescript
private subscribeToAssignments(): void {
  this.calendarBuilder.assignments$
    .pipe(takeUntil(this.destroy$))
    .subscribe((assignments) => {
      this.assignments = assignments;
      this.duplicateImageIds = this.calendarBuilder.getDuplicateImageIds();
    });
}
```

#### 3. CalendarPreviewComponent збереження customization

**Auto-save customization в localStorage** (calendar-preview.component.ts:229-257):

```typescript
private loadCustomization(): void {
  const saved = localStorage.getItem('calendar-customization');
  if (saved) {
    this.customization = JSON.parse(saved);
  }
}

onCustomizationChange(): void {
  this.saveCustomization(); // Auto-save при кожній зміні
}

private saveCustomization(): void {
  localStorage.setItem(
    'calendar-customization',
    JSON.stringify(this.customization)
  );
}
```

### Не реалізовано ❌:

#### Undo/Redo для calendar assignments

**Чого немає:**
- ❌ History stack для calendar operations (assignImageToMonth, removeAssignment)
- ❌ Keyboard shortcuts (Ctrl+Z, Ctrl+Y) для календаря
- ❌ Undo/Redo UI buttons

**Примітка:** EditorStateService має undo/redo, але він для image editing (rotate, crop), а не для calendar assignments.

**Для реалізації потрібно:**
1. Додати history в CalendarBuilderService
2. Зберігати snapshots assignments перед кожною зміною
3. Додати undo()/redo() методи
4. Додати keyboard shortcuts в EditorComponent

#### Backend збереження

**Чого немає:**
- ❌ Manual "Save" кнопка
- ❌ PUT /api/calendars/{id} endpoint для збереження
- ❌ Збереження assignments на backend
- ❌ Preview image generation та upload

### Використані технології:

- **localStorage API** - persistence
- **RxJS BehaviorSubject** - reactive state
- **TypeScript** - type safety
- **Angular** - framework

### Файли:

**Core Service:**
- `/src/Calendary.Ng/src/app/pages/editor/services/calendar-builder.service.ts` - auto-save logic

**Components:**
- `/src/Calendary.Ng/src/app/pages/editor/components/calendar-preview/calendar-preview.component.ts` - customization save
- `/src/Calendary.Ng/src/app/pages/editor/editor.component.ts` - state subscription

### Критерії успіху:

#### Виконано ✅:
- ✅ **Auto-save в localStorage** - assignments зберігаються автоматично
- ✅ **Відновлення при перезавантаженні** - loadFromStorage() при init
- ✅ Reactive state через BehaviorSubject
- ✅ Customization auto-save (fonts, colors)
- ✅ Error handling (silent fails для storage errors)

#### Не виконано ❌:
- ❌ **Undo/Redo для calendar assignments**
- ❌ Keyboard shortcuts (Ctrl+Z, Ctrl+Y)
- ❌ Manual save кнопка
- ❌ Backend збереження (PUT /api/calendars/{id})
- ❌ Preview image generation
- ❌ "Saving..." індикатор

### Архітектура:

```
CalendarBuilderService (Singleton)
    ├── BehaviorSubject<MonthAssignment[]>
    ├── localStorage persistence
    └── Methods:
        ├── assignImageToMonth() → auto-save ✅
        ├── removeAssignment() → auto-save ✅
        ├── clear() → auto-save ✅
        ├── loadFromStorage() → auto-restore ✅
        └── persist() → write to localStorage ✅

EditorComponent
    └── subscribe to assignments$ → reactive UI updates ✅
```

### Примітки:

**Що працює:**
1. ✅ Automatic localStorage persistence - assignments не втрачаються
2. ✅ Auto-restore при завантаженні сторінки
3. ✅ Reactive updates через RxJS
4. ✅ Customization збереження

**Що треба додати:**
1. ❌ Undo/Redo функціонал для calendar operations
2. ❌ Backend integration для збереження на сервер
3. ❌ Preview image generation (для PDF export)

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: 2025-11-16
