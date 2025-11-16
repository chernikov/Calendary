# Task 13: Drag & Drop для зображень на canvas

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Claude
**Залежить від**: Task 12, 06
**Паралельно з**: Task 09, 10, 11

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

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
