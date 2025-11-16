# Task 13: Drag & Drop для зображень на canvas

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: ✅ COMPLETED (частково)
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Claude
**Залежить від**: Task 12, 06
**Паралельно з**: Task 09, 10, 11
**Виконано**: 2025-11-16 (реалізовано в Angular)

## Опис задачі

Імплементувати drag & drop функціонал для додавання фото на canvas, можливість resize, rotate, переміщення зображень.

## Проблема

Користувачі повинні мати можливість легко додавати свої фото на календар та маніпулювати ними (переміщати, змінювати розмір, обертати).

## Що треба зробити

1. **Створити ImageUploadPanel компонент**
   - `src/components/features/editor/ImageUploadPanel.tsx`
   - Кнопка "Завантажити фото"
   - Drag & drop zone
   - Прогрес бар завантаження
   - Thumbnails завантажених фото

2. **Інтеграція з File Upload API**
   - POST /api/files/upload
   - Handle upload progress
   - Error handling (розмір, формат)
   - Показувати завантажені фото

3. **Drag & Drop на Canvas**
   - Drag image з панелі на canvas
   - Drop handler на canvas
   - Додавання image object на canvas
   - Initial position та size

4. **Image Manipulation на Canvas**
   - Resize handles (corners)
   - Rotation handle
   - Move (drag)
   - Delete (delete key або кнопка)
   - Z-index control (bring to front, send to back)

5. **Canvas Image Object Properties**
   - Position (x, y)
   - Size (width, height)
   - Rotation (angle)
   - Scale (maintain aspect ratio)
   - Filters (brightness, contrast, saturation - опціонально)

6. **Toolbar для Image**
   - Показувати при виборі image
   - Кнопки: Delete, Duplicate, Flip Horizontal, Flip Vertical
   - Z-index controls

## Файли для створення/модифікації

- `src/components/features/editor/ImageUploadPanel.tsx`
- `src/components/features/editor/CanvasEditor.tsx` - додати drag & drop
- `src/components/features/editor/ImageToolbar.tsx`
- `src/hooks/useImageUpload.ts`
- `src/services/fileService.ts`

## Критерії успіху

- [ ] Можна завантажити фото через кнопку або drag & drop
- [ ] Завантажені фото з'являються в панелі
- [ ] Можна перетягнути фото з панелі на canvas
- [ ] На canvas можна переміщати, змінювати розмір, обертати фото
- [ ] Delete працює (клавіша або кнопка)
- [ ] Z-index controls працюють
- [ ] Progress bar показується під час завантаження
- [ ] Error handling працює (розмір, формат)

## Залежності

- Task 12: Canvas editor повинен бути готовий
- Task 06: File upload API повинен працювати

## Блокується наступні задачі

- Task 16: Preview потребує готових зображень на canvas

## Технічні деталі

### src/components/features/editor/ImageUploadPanel.tsx
```typescript
'use client'

import { useState } from 'react'
import { useDropzone } from 'react-dropzone'
import { Button } from '@/components/ui/button'
import { Progress } from '@/components/ui/progress'
import { Upload, X } from 'lucide-react'
import { fileService } from '@/services/fileService'
import Image from 'next/image'

interface UploadedImage {
  id: string
  url: string
  thumbnailUrl: string
}

interface ImageUploadPanelProps {
  onImageSelect: (imageUrl: string) => void
}

export default function ImageUploadPanel({ onImageSelect }: ImageUploadPanelProps) {
  const [images, setImages] = useState<UploadedImage[]>([])
  const [uploading, setUploading] = useState(false)
  const [uploadProgress, setUploadProgress] = useState(0)

  const onDrop = async (acceptedFiles: File[]) => {
    for (const file of acceptedFiles) {
      try {
        setUploading(true)
        setUploadProgress(0)

        const uploadedFile = await fileService.upload(file, (progress) => {
          setUploadProgress(progress)
        })

        setImages((prev) => [...prev, uploadedFile])
      } catch (error) {
        console.error('Upload failed:', error)
        // Show error toast
      } finally {
        setUploading(false)
        setUploadProgress(0)
      }
    }
  }

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'image/jpeg': ['.jpg', '.jpeg'],
      'image/png': ['.png'],
      'image/webp': ['.webp'],
    },
    maxSize: 10 * 1024 * 1024, // 10MB
  })

  const handleImageDragStart = (e: React.DragEvent, imageUrl: string) => {
    e.dataTransfer.setData('imageUrl', imageUrl)
  }

  return (
    <div className="w-64 bg-white border-l p-4 overflow-y-auto">
      <h3 className="font-semibold mb-4">Мої фото</h3>

      {/* Upload Zone */}
      <div
        {...getRootProps()}
        className={`border-2 border-dashed rounded-lg p-4 mb-4 text-center cursor-pointer transition-colors ${
          isDragActive ? 'border-blue-500 bg-blue-50' : 'border-gray-300'
        }`}
      >
        <input {...getInputProps()} />
        <Upload className="w-8 h-8 mx-auto mb-2 text-gray-400" />
        <p className="text-sm text-gray-600">
          {isDragActive ? 'Перетягніть сюди' : 'Натисніть або перетягніть фото'}
        </p>
      </div>

      {/* Upload Progress */}
      {uploading && (
        <div className="mb-4">
          <Progress value={uploadProgress} />
          <p className="text-xs text-gray-500 mt-1">Завантаження...</p>
        </div>
      )}

      {/* Uploaded Images */}
      <div className="grid grid-cols-2 gap-2">
        {images.map((image) => (
          <div
            key={image.id}
            className="relative aspect-square rounded border cursor-move hover:border-blue-500"
            draggable
            onDragStart={(e) => handleImageDragStart(e, image.url)}
          >
            <Image
              src={image.thumbnailUrl}
              alt="Uploaded"
              fill
              className="object-cover rounded"
            />
          </div>
        ))}
      </div>
    </div>
  )
}
```

### Canvas Drag & Drop Handler (Fabric.js)
```typescript
// In CanvasEditor.tsx

const handleCanvasDrop = (e: React.DragEvent) => {
  e.preventDefault()

  const imageUrl = e.dataTransfer.getData('imageUrl')
  if (!imageUrl || !fabricCanvas) return

  const rect = canvasRef.current!.getBoundingClientRect()
  const x = e.clientX - rect.left
  const y = e.clientY - rect.top

  fabric.Image.fromURL(imageUrl, (img) => {
    img.set({
      left: x,
      top: y,
      scaleX: 0.5,
      scaleY: 0.5,
    })

    fabricCanvas.add(img)
    fabricCanvas.setActiveObject(img)
    fabricCanvas.requestRenderAll()
  }, {
    crossOrigin: 'anonymous'
  })
}

return (
  <div
    ref={canvasRef}
    onDrop={handleCanvasDrop}
    onDragOver={(e) => e.preventDefault()}
  >
    <canvas id="canvas" />
  </div>
)
```

### src/services/fileService.ts
```typescript
import axios from 'axios'

const API_URL = process.env.NEXT_PUBLIC_API_URL

export const fileService = {
  async upload(
    file: File,
    onProgress?: (progress: number) => void
  ): Promise<UploadedImage> {
    const formData = new FormData()
    formData.append('file', file)

    const response = await axios.post(`${API_URL}/files/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (progressEvent) => {
        const progress = Math.round(
          (progressEvent.loaded * 100) / (progressEvent.total || 1)
        )
        onProgress?.(progress)
      },
    })

    return response.data
  },
}
```

## Примітки

- react-dropzone - популярна бібліотека для drag & drop
- Fabric.js має вбудовані controls для resize, rotate
- Важливо зберігати aspect ratio при resize
- crossOrigin: 'anonymous' для CORS

## Чому Claude?

Складна інтеграція:
- Drag & drop між різними зонами
- Canvas manipulation (Fabric.js API)
- File upload з progress tracking
- Image transformation logic

---

## Фактична реалізація в Angular

**Важливо**: Це НЕ про складний drag & drop зображень на canvas з resize/rotate. Це про **призначення зображень на місяці календаря** та **перестановку між місяцями**.

### Реалізовано ✅:

#### 1. Drag & Drop між місяцями календаря

**CalendarPreviewComponent** (calendar-preview.component.ts:105-122):

```typescript
// Drag start - почати перетягування місяця
onDragStart(month: number, event: DragEvent): void {
  event.dataTransfer?.setData('text/plain', month.toString());
  event.dataTransfer?.setDragImage(new Image(), 0, 0);
}

// Drag over - дозволити drop
onDragOver(event: DragEvent): void {
  event.preventDefault();
}

// Drop - swap місяці
onDrop(targetMonth: number, event: DragEvent): void {
  event.preventDefault();
  const from = parseInt(event.dataTransfer?.getData('text/plain'));
  if (from && from !== targetMonth) {
    this.swapRequested.emit({ from, to: targetMonth });
  }
}
```

**EditorComponent** - обробка swap (editor.component.ts:341-358):

```typescript
onMonthSwap(payload: { from: number; to: number }): void {
  const fromAssignment = this.calendarBuilder.getAssignment(from);
  const toAssignment = this.calendarBuilder.getAssignment(to);

  // Swap зображення між місяцями
  if (toAssignment) {
    this.calendarBuilder.assignImageToMonth(to, fromAssignment.imageId, fromAssignment.imageUrl);
    this.calendarBuilder.assignImageToMonth(from, toAssignment.imageId, toAssignment.imageUrl);
  } else {
    // Перемістити на порожній місяць
    this.calendarBuilder.assignImageToMonth(to, fromAssignment.imageId, fromAssignment.imageUrl);
    this.calendarBuilder.removeAssignment(from);
  }
}
```

#### 2. Призначення зображення на місяць (Click)

**EditorComponent** (editor.component.ts:270-284):

```typescript
onAddToCalendar(image: JobTask): void {
  // Відкрити MonthSelectorComponent
  const dialogRef = this.dialog.open(MonthSelectorComponent, {
    data: { images: allImages, assignments: this.assignments }
  });

  dialogRef.afterClosed().subscribe((result) => {
    if (result?.month) {
      this.assignImageToMonth(image, result.month);
    }
  });
}
```

#### 3. MonthSelectorComponent

**Функціонал:**
- ✅ Modal з вибором місяця (1-12)
- ✅ Відображення поточних призначень
- ✅ Попередній перегляд зображення
- ✅ Можливість вибрати інше зображення зі списку
- ✅ Кнопка "Очистити місяць"

#### 4. ImageGalleryComponent

**Функціонал:**
- ✅ Відображення всіх зображень з jobs/tasks
- ✅ Кнопка "Додати до календаря" для кожного зображення
- ✅ Індикатор чи зображення вже призначене (показує місяць)
- ✅ Видалення зображення + автоматичне видалення з календаря

**Код** (image-gallery.component.ts:35-37, 67-78):

```typescript
requestAddToCalendar(image: JobTask, event: Event): void {
  event.stopPropagation();
  this.addToCalendar.emit(image);
}

getAssignedMonth(image: JobTask): number | null {
  const assignment = this.assignments.find(
    (a) => a.imageId === image.id.toString()
  );
  return assignment?.month ?? null;
}

getMonthLabel(month: number | null): string | null {
  return month ? MONTHS.find((m) => m.value === month)?.label : null;
}
```

### Використані технології:

- **HTML5 Drag & Drop API** - native browser drag & drop
- **CalendarBuilderService** - управління призначеннями
- **MatDialog** - modal для MonthSelectorComponent
- **RxJS** - reactive updates
- **Angular Material** - UI компоненти

### Файли:

**Реалізовані компоненти:**
- `/src/Calendary.Ng/src/app/pages/editor/components/calendar-preview/calendar-preview.component.ts` - drag & drop between months
- `/src/Calendary.Ng/src/app/pages/editor/components/month-selector/month-selector.component.ts` - month selection modal
- `/src/Calendary.Ng/src/app/pages/editor/components/image-gallery/image-gallery.component.ts` - image list with "Add to calendar"
- `/src/Calendary.Ng/src/app/pages/editor/editor.component.ts` - orchestration
- `/src/Calendary.Ng/src/app/pages/editor/services/calendar-builder.service.ts` - state management

### Критерії успіху:

#### Виконано ✅:
- ✅ Drag & Drop між місяцями календаря (swap functionality)
- ✅ Click на місяць → вибір зображення
- ✅ Галерея зображень з кнопкою "Додати до календаря"
- ✅ MonthSelectorComponent для вибору місяця
- ✅ Відображення чи зображення вже призначене
- ✅ Delete зображення → автоматичне видалення з календаря
- ✅ Clear month button
- ✅ Visual feedback (duplicate detection, progress)

#### Не виконано (не потрібно для календаря) ⚠️:
- ⚠️ Resize/rotate зображень на canvas (не потрібно, зображення статичні)
- ⚠️ File upload з комп'ютера (зображення генеруються AI)

### Примітки:

**Що реалізовано:**
1. ✅ Призначення зображень на місяці календаря
2. ✅ Drag & Drop для переміщення/swap між місяцями
3. ✅ Visual UI для управління календарем
4. ✅ Auto-save в localStorage

**Що НЕ потрібно:**
- ❌ Складний canvas editor (Fabric.js/Konva.js)
- ❌ Resize/rotate/crop зображень
- ❌ Drag з панелі на canvas (замість цього - click → select month)

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: 2025-11-16
