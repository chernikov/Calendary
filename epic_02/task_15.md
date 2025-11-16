# Task 15: Undo/Redo та збереження стану

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Claude
**Залежить від**: Task 12, 13, 14

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

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
