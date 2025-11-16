# Task 14: Інструменти редагування (текст, фігури)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: ✅ DONE (Calendary.Ng)
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Codex
**Залежить від**: Task 12
**Паралельно з**: Task 09, 10, 11

## Опис задачі

Додати інструменти для редагування тексту (додавання, зміна шрифту, кольору) та додавання фігур (прямокутник, коло, лінія).

## Проблема

Користувачі повинні мати можливість додавати текст та декоративні елементи на свій календар.

## Що треба зробити

1. **Створити EditorToolbar компонент**
   - `src/components/features/editor/EditorToolbar.tsx`
   - Кнопки для інструментів:
     - Додати текст
     - Додати прямокутник
     - Додати коло
     - Додати лінію

2. **Додати Text Tool**
   - Кнопка "Додати текст"
   - Створення text object на canvas
   - Editable text (подвійний клік для редагування)
   - Text toolbar при виборі:
     - Font family dropdown
     - Font size input
     - Bold, Italic, Underline
     - Text color picker
     - Text align (left, center, right)

3. **Додати Shape Tools**
   - Rectangle tool - додати прямокутник
   - Circle tool - додати коло
   - Line tool - додати лінію
   - Shape toolbar при виборі:
     - Fill color picker
     - Stroke color picker
     - Stroke width
     - Opacity slider

4. **Створити Font Selector**
   - Dropdown з популярними шрифтами
   - Google Fonts integration
   - Preview шрифтів

5. **Створити Color Picker**
   - `src/components/ui/ColorPicker.tsx`
   - Palette з preset кольорами
   - Custom color picker
   - Recent colors

6. **Контекстна панель властивостей**
   - Показувати properties для вибраного object
   - Різні properties для text/shape/image
   - Real-time updates

## Файли для створення/модифікації

- `src/components/features/editor/EditorToolbar.tsx`
- `src/components/features/editor/TextToolbar.tsx`
- `src/components/features/editor/ShapeToolbar.tsx`
- `src/components/features/editor/FontSelector.tsx`
- `src/components/ui/ColorPicker.tsx`
- `src/components/features/editor/CanvasEditor.tsx` - інтеграція tools

## Критерії успіху

- [ ] Можна додати текст на canvas
- [ ] Можна редагувати текст (подвійний клік)
- [ ] Можна змінити шрифт, розмір, колір тексту
- [ ] Можна додати фігури (прямокутник, коло, лінія)
- [ ] Можна змінити колір заливки та обведення фігур
- [ ] Color picker працює для всіх елементів
- [ ] Properties panel оновлюється при виборі object

## Залежності

- Task 12: Canvas editor повинен бути готовий

## Блокується наступні задачі

- Task 15: Undo/Redo потребує всіх інструментів редагування

## Технічні деталі

### src/components/features/editor/EditorToolbar.tsx
```typescript
'use client'

import { Button } from '@/components/ui/button'
import { Type, Square, Circle, Minus } from 'lucide-react'

interface EditorToolbarProps {
  onAddText: () => void
  onAddRectangle: () => void
  onAddCircle: () => void
  onAddLine: () => void
}

export default function EditorToolbar({
  onAddText,
  onAddRectangle,
  onAddCircle,
  onAddLine,
}: EditorToolbarProps) {
  return (
    <div className="bg-white border-b p-2 flex gap-2">
      <Button variant="outline" size="sm" onClick={onAddText}>
        <Type className="w-4 h-4 mr-2" />
        Текст
      </Button>

      <div className="border-l mx-2" />

      <Button variant="outline" size="sm" onClick={onAddRectangle}>
        <Square className="w-4 h-4 mr-2" />
        Прямокутник
      </Button>

      <Button variant="outline" size="sm" onClick={onAddCircle}>
        <Circle className="w-4 h-4 mr-2" />
        Коло
      </Button>

      <Button variant="outline" size="sm" onClick={onAddLine}>
        <Minus className="w-4 h-4 mr-2" />
        Лінія
      </Button>
    </div>
  )
}
```

### Canvas Tools Implementation (Fabric.js)
```typescript
// In CanvasEditor.tsx

const addText = () => {
  const text = new fabric.IText('Ваш текст', {
    left: 100,
    top: 100,
    fontFamily: 'Arial',
    fontSize: 24,
    fill: '#000000',
  })

  fabricCanvas.add(text)
  fabricCanvas.setActiveObject(text)
  fabricCanvas.requestRenderAll()
}

const addRectangle = () => {
  const rect = new fabric.Rect({
    left: 100,
    top: 100,
    width: 200,
    height: 100,
    fill: '#3b82f6',
    stroke: '#1e40af',
    strokeWidth: 2,
  })

  fabricCanvas.add(rect)
  fabricCanvas.setActiveObject(rect)
  fabricCanvas.requestRenderAll()
}

const addCircle = () => {
  const circle = new fabric.Circle({
    left: 100,
    top: 100,
    radius: 50,
    fill: '#ef4444',
    stroke: '#991b1b',
    strokeWidth: 2,
  })

  fabricCanvas.add(circle)
  fabricCanvas.setActiveObject(circle)
  fabricCanvas.requestRenderAll()
}

const addLine = () => {
  const line = new fabric.Line([50, 100, 200, 100], {
    stroke: '#000000',
    strokeWidth: 3,
  })

  fabricCanvas.add(line)
  fabricCanvas.setActiveObject(line)
  fabricCanvas.requestRenderAll()
}
```

### src/components/features/editor/TextToolbar.tsx
```typescript
'use client'

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Bold, Italic, Underline } from 'lucide-react'
import ColorPicker from '@/components/ui/ColorPicker'

const FONTS = [
  'Arial',
  'Times New Roman',
  'Courier New',
  'Georgia',
  'Verdana',
  'Helvetica',
  'Comic Sans MS',
]

interface TextToolbarProps {
  selectedText: fabric.IText | null
  onUpdate: (property: string, value: any) => void
}

export default function TextToolbar({ selectedText, onUpdate }: TextToolbarProps) {
  if (!selectedText) return null

  return (
    <div className="bg-white border-t p-3 flex gap-3 items-center">
      <Select
        value={selectedText.fontFamily}
        onValueChange={(value) => onUpdate('fontFamily', value)}
      >
        <SelectTrigger className="w-40">
          <SelectValue placeholder="Шрифт" />
        </SelectTrigger>
        <SelectContent>
          {FONTS.map((font) => (
            <SelectItem key={font} value={font}>
              {font}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Input
        type="number"
        value={selectedText.fontSize}
        onChange={(e) => onUpdate('fontSize', parseInt(e.target.value))}
        className="w-20"
      />

      <Button
        variant={selectedText.fontWeight === 'bold' ? 'default' : 'outline'}
        size="sm"
        onClick={() => onUpdate('fontWeight', selectedText.fontWeight === 'bold' ? 'normal' : 'bold')}
      >
        <Bold className="w-4 h-4" />
      </Button>

      <Button
        variant={selectedText.fontStyle === 'italic' ? 'default' : 'outline'}
        size="sm"
        onClick={() => onUpdate('fontStyle', selectedText.fontStyle === 'italic' ? 'normal' : 'italic')}
      >
        <Italic className="w-4 h-4" />
      </Button>

      <ColorPicker
        color={selectedText.fill as string}
        onChange={(color) => onUpdate('fill', color)}
      />
    </div>
  )
}
```

### src/components/ui/ColorPicker.tsx
```typescript
'use client'

import { useState } from 'react'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { Button } from '@/components/ui/button'

const PRESET_COLORS = [
  '#000000', '#FFFFFF', '#FF0000', '#00FF00', '#0000FF',
  '#FFFF00', '#FF00FF', '#00FFFF', '#FFA500', '#800080',
]

interface ColorPickerProps {
  color: string
  onChange: (color: string) => void
}

export default function ColorPicker({ color, onChange }: ColorPickerProps) {
  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="outline" size="sm" className="w-10 h-10 p-0">
          <div
            className="w-6 h-6 rounded border"
            style={{ backgroundColor: color }}
          />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-64">
        <div className="grid grid-cols-5 gap-2">
          {PRESET_COLORS.map((presetColor) => (
            <button
              key={presetColor}
              className="w-10 h-10 rounded border hover:scale-110 transition-transform"
              style={{ backgroundColor: presetColor }}
              onClick={() => onChange(presetColor)}
            />
          ))}
        </div>

        <div className="mt-3">
          <input
            type="color"
            value={color}
            onChange={(e) => onChange(e.target.value)}
            className="w-full h-10"
          />
        </div>
      </PopoverContent>
    </Popover>
  )
}
```

## Примітки

- fabric.IText дозволяє редагування тексту подвійним кліком
- Google Fonts можна підключити через next/font
- Color picker можна покращити бібліотекою react-colorful
- У `Calendary.Ng` додано власний Canvas overlay сервіс, панель властивостей і кнопки у Toolbar для тексту, фігур та ліній.

## Чому Codex?

UI інструменти задача:
- Стандартні toolbar компоненти
- Form controls (select, input, buttons)
- Fabric.js API calls (документовані)
- Типовий editor UI pattern

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
