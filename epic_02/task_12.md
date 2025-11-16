# Task 12: Canvas Editor Setup (Fabric.js/Konva.js)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 6-8 годин
**Відповідальний AI**: Claude
**Залежить від**: Task 03, 05
**Паралельно з**: Task 09, 10, 11

## Опис задачі

Вибрати та інтегрувати canvas бібліотеку (Fabric.js або Konva.js) для створення інтерактивного редактора календарів. Це ключовий компонент Customer Portal, який дозволяє користувачам персоналізувати календарі.

## Проблема

Потрібен потужний та зручний редактор, який дозволяє:
- Завантажувати та розміщувати зображення
- Додавати текст з різними шрифтами
- Масштабувати, обертати, переміщувати об'єкти
- Зберігати стан редактора
- Експортувати результат у зображення

## Дослідження: Fabric.js vs Konva.js

### Fabric.js
**Переваги:**
- Більше features "з коробки"
- Краща підтримка SVG
- Більша спільнота
- Простіша робота з текстом

**Недоліки:**
- Більший розмір бандлу
- Старіший API
- TypeScript підтримка не ідеальна

### Konva.js
**Переваги:**
- Кращий TypeScript support
- Менший розмір
- Кращий performance
- Сучасніший API

**Недоліки:**
- Менше готових features
- Меншаspільнота

**Рекомендація**: **Fabric.js** - більше підходить для calendar editor через краще handling тексту та зображень.

## Що треба зробити

### 1. Встановити залежності

```bash
npm install fabric
npm install --save-dev @types/fabric
```

### 2. Створити Canvas компонент

**components/editor/CalendarCanvas.tsx:**
```typescript
'use client';

import { useEffect, useRef, useState } from 'react';
import { fabric } from 'fabric';

interface CalendarCanvasProps {
  templateData?: any;
  width?: number;
  height?: number;
  onCanvasReady?: (canvas: fabric.Canvas) => void;
  onCanvasChange?: (json: string) => void;
}

export function CalendarCanvas({
  templateData,
  width = 800,
  height = 1200,
  onCanvasReady,
  onCanvasChange,
}: CalendarCanvasProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [canvas, setCanvas] = useState<fabric.Canvas | null>(null);

  useEffect(() => {
    if (!canvasRef.current) return;

    // Ініціалізувати Fabric.js canvas
    const fabricCanvas = new fabric.Canvas(canvasRef.current, {
      width,
      height,
      backgroundColor: '#ffffff',
      preserveObjectStacking: true,
    });

    // Налаштувати grid (опціонально)
    // addGrid(fabricCanvas);

    // Завантажити template data якщо є
    if (templateData) {
      fabricCanvas.loadFromJSON(templateData, () => {
        fabricCanvas.renderAll();
      });
    }

    // Callback коли canvas готовий
    onCanvasReady?.(fabricCanvas);
    setCanvas(fabricCanvas);

    // Event listeners для змін
    fabricCanvas.on('object:modified', handleCanvasChange);
    fabricCanvas.on('object:added', handleCanvasChange);
    fabricCanvas.on('object:removed', handleCanvasChange);

    // Cleanup
    return () => {
      fabricCanvas.dispose();
    };
  }, []);

  const handleCanvasChange = () => {
    if (!canvas) return;
    const json = JSON.stringify(canvas.toJSON());
    onCanvasChange?.(json);
  };

  return (
    <div className="relative border border-gray-300 shadow-lg">
      <canvas ref={canvasRef} />
    </div>
  );
}
```

### 3. Створити Canvas Context

**contexts/CanvasContext.tsx:**
```typescript
'use client';

import { createContext, useContext, useState, ReactNode } from 'react';
import { fabric } from 'fabric';

interface CanvasContextType {
  canvas: fabric.Canvas | null;
  setCanvas: (canvas: fabric.Canvas | null) => void;
  currentTool: 'select' | 'text' | 'image' | 'shape';
  setCurrentTool: (tool: 'select' | 'text' | 'image' | 'shape') => void;
}

const CanvasContext = createContext<CanvasContextType | undefined>(undefined);

export function CanvasProvider({ children }: { children: ReactNode }) {
  const [canvas, setCanvas] = useState<fabric.Canvas | null>(null);
  const [currentTool, setCurrentTool] = useState<'select' | 'text' | 'image' | 'shape'>('select');

  return (
    <CanvasContext.Provider value={{ canvas, setCanvas, currentTool, setCurrentTool }}>
      {children}
    </CanvasContext.Provider>
  );
}

export function useCanvas() {
  const context = useContext(CanvasContext);
  if (!context) {
    throw new Error('useCanvas must be used within CanvasProvider');
  }
  return context;
}
```

### 4. Створити Canvas utilities

**lib/canvas/utils.ts:**
```typescript
import { fabric } from 'fabric';

export class CanvasUtils {
  // Додати зображення на canvas
  static addImage(canvas: fabric.Canvas, imageUrl: string) {
    fabric.Image.fromURL(imageUrl, (img) => {
      img.scale(0.5);
      img.set({
        left: 100,
        top: 100,
      });
      canvas.add(img);
      canvas.setActiveObject(img);
      canvas.renderAll();
    });
  }

  // Додати текст
  static addText(canvas: fabric.Canvas, text: string = 'Текст') {
    const textObj = new fabric.IText(text, {
      left: 100,
      top: 100,
      fontFamily: 'Arial',
      fontSize: 24,
      fill: '#000000',
    });
    canvas.add(textObj);
    canvas.setActiveObject(textObj);
    canvas.renderAll();
  }

  // Додати прямокутник
  static addRectangle(canvas: fabric.Canvas) {
    const rect = new fabric.Rect({
      left: 100,
      top: 100,
      fill: '#cccccc',
      width: 200,
      height: 100,
      stroke: '#000000',
      strokeWidth: 2,
    });
    canvas.add(rect);
    canvas.setActiveObject(rect);
    canvas.renderAll();
  }

  // Видалити вибраний об'єкт
  static deleteSelected(canvas: fabric.Canvas) {
    const activeObject = canvas.getActiveObject();
    if (activeObject) {
      canvas.remove(activeObject);
      canvas.renderAll();
    }
  }

  // Експортувати canvas в JSON
  static exportToJSON(canvas: fabric.Canvas): string {
    return JSON.stringify(canvas.toJSON());
  }

  // Експортувати canvas в зображення
  static exportToImage(canvas: fabric.Canvas, format: 'png' | 'jpeg' = 'png'): string {
    return canvas.toDataURL({
      format,
      quality: 1,
      multiplier: 2, // 2x для кращої якості
    });
  }

  // Завантажити з JSON
  static loadFromJSON(canvas: fabric.Canvas, json: string) {
    canvas.loadFromJSON(json, () => {
      canvas.renderAll();
    });
  }

  // Очистити canvas
  static clearCanvas(canvas: fabric.Canvas) {
    canvas.clear();
    canvas.backgroundColor = '#ffffff';
    canvas.renderAll();
  }

  // Центрувати об'єкт
  static centerObject(canvas: fabric.Canvas, obj: fabric.Object) {
    obj.center();
    canvas.renderAll();
  }
}
```

### 5. Створити Toolbar компонент

**components/editor/Toolbar.tsx:**
```typescript
'use client';

import { useCanvas } from '@/contexts/CanvasContext';
import { Button } from '@/components/ui/button';
import {
  Type,
  Image,
  Square,
  MousePointer,
  Trash2,
  Download
} from 'lucide-react';
import { CanvasUtils } from '@/lib/canvas/utils';

export function Toolbar() {
  const { canvas, currentTool, setCurrentTool } = useCanvas();

  const handleAddText = () => {
    if (!canvas) return;
    CanvasUtils.addText(canvas, 'Новий текст');
    setCurrentTool('text');
  };

  const handleAddShape = () => {
    if (!canvas) return;
    CanvasUtils.addRectangle(canvas);
    setCurrentTool('shape');
  };

  const handleDelete = () => {
    if (!canvas) return;
    CanvasUtils.deleteSelected(canvas);
  };

  const handleExport = () => {
    if (!canvas) return;
    const dataUrl = CanvasUtils.exportToImage(canvas, 'png');
    // Завантажити файл
    const link = document.createElement('a');
    link.download = 'calendar.png';
    link.href = dataUrl;
    link.click();
  };

  return (
    <div className="flex gap-2 p-4 bg-white border-b">
      <Button
        variant={currentTool === 'select' ? 'default' : 'outline'}
        onClick={() => setCurrentTool('select')}
      >
        <MousePointer className="w-4 h-4 mr-2" />
        Вибрати
      </Button>

      <Button
        variant={currentTool === 'text' ? 'default' : 'outline'}
        onClick={handleAddText}
      >
        <Type className="w-4 h-4 mr-2" />
        Текст
      </Button>

      <Button
        variant={currentTool === 'image' ? 'default' : 'outline'}
        onClick={() => setCurrentTool('image')}
      >
        <Image className="w-4 h-4 mr-2" />
        Зображення
      </Button>

      <Button
        variant={currentTool === 'shape' ? 'default' : 'outline'}
        onClick={handleAddShape}
      >
        <Square className="w-4 h-4 mr-2" />
        Фігури
      </Button>

      <div className="ml-auto flex gap-2">
        <Button variant="destructive" onClick={handleDelete}>
          <Trash2 className="w-4 h-4 mr-2" />
          Видалити
        </Button>

        <Button onClick={handleExport}>
          <Download className="w-4 h-4 mr-2" />
          Експорт
        </Button>
      </div>
    </div>
  );
}
```

### 6. Створити сторінку редактора

**app/editor/[templateId]/page.tsx:**
```typescript
'use client';

import { CanvasProvider } from '@/contexts/CanvasContext';
import { CalendarCanvas } from '@/components/editor/CalendarCanvas';
import { Toolbar } from '@/components/editor/Toolbar';
import { useParams } from 'next/navigation';
import { useEffect, useState } from 'react';

export default function EditorPage() {
  const params = useParams();
  const templateId = params.templateId as string;
  const [templateData, setTemplateData] = useState(null);

  useEffect(() => {
    // Завантажити template data з API
    // fetch(`/api/templates/${templateId}`)
    //   .then(res => res.json())
    //   .then(data => setTemplateData(data.templateDataJson));
  }, [templateId]);

  const handleCanvasChange = (json: string) => {
    // Auto-save logic
    console.log('Canvas changed:', json);
  };

  return (
    <CanvasProvider>
      <div className="flex flex-col h-screen">
        <Toolbar />
        <div className="flex-1 flex items-center justify-center bg-gray-100 p-8">
          <CalendarCanvas
            templateData={templateData}
            onCanvasChange={handleCanvasChange}
          />
        </div>
      </div>
    </CanvasProvider>
  );
}
```

## Критерії успіху

- [x] Fabric.js встановлено та налаштовано
- [x] Canvas компонент створено та працює
- [x] Canvas Context для управління станом
- [x] Утиліти для роботи з canvas (CanvasUtils)
- [x] Toolbar з базовими інструментами
- [x] Можна додавати текст, фігури
- [x] Можна видаляти об'єкти
- [x] Експорт в зображення працює
- [x] Збереження/завантаження з JSON
- [x] TypeScript типізація

## Залежності

- Task 03: Layout та routing (потрібен для сторінки editor)
- Task 05: API для calendars (потрібен для збереження)

## Блокує наступні задачі

- Task 13: Drag & Drop зображень (потребує canvas setup)
- Task 14: Інструменти редагування (потребує canvas)
- Task 15: Undo/Redo (потребує canvas)

## Примітки

### Чому Claude?

Ця задача потребує:
- **Архітектурного рішення**: вибір між Fabric.js та Konva.js
- **Глибокого розуміння**: як працювати з canvas API
- **Проектування**: структура компонентів, context, utilities
- **Performance considerations**: як оптимізувати canvas операції

Claude краще справляється з такими складними задачами.

### Performance tips

- Використовувати `fabric.Object.NUM_FRACTION_DIGITS = 2` для зменшення розміру JSON
- Debounce для auto-save
- `canvas.renderOnAddRemove = false` під час bulk операцій
- `canvas.requestRenderAll()` замість `renderAll()` де можливо

### Майбутні покращення (не в цій задачі)

- Zoom in/out
- Pan (переміщення view)
- Snap to grid
- Layers management
- Keyboard shortcuts (Ctrl+Z, Delete, etc.)

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
