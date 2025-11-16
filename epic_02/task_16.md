# Task 16: Попередній перегляд готового календаря

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: ✅ DONE (Calendary.Ng)
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 3-4 години
**Відповідальний AI**: Codex
**Залежить від**: Task 12, 13, 14

## Опис задачі

Створити preview modal для перегляду готового календаря, експорт canvas в зображення, кнопка "Додати в кошик".

## Проблема

Користувачі повинні мати можливість подивитися на фінальний вигляд календаря перед додаванням в кошик.

## Що треба зробити

1. **Створити PreviewModal компонент**
   - `src/components/features/editor/PreviewModal.tsx`
   - Full-screen preview canvas
   - High-quality render
   - Кнопка "Додати в кошик"
   - Кнопка "Завантажити як зображення" (опціонально)

2. **Експорт canvas в high-quality image**
   - Canvas.toDataURL() з multiplier для якості
   - Export в PNG або JPEG
   - Розмір для друку (300 DPI)

3. **Додати Preview кнопку в Editor**
   - В EditorToolbar
   - Відкриває PreviewModal
   - Показувати попередній розмір календаря

4. **Format selection в Preview**
   - Dropdown для вибору формату (A3, A4)
   - Dropdown для paper type (Glossy, Matte)
   - Показувати ціну залежно від вибору
   - Quantity selector

5. **Додати в кошик**
   - POST /api/cart/items
   - Передати calendarId, format, paperType, quantity
   - Redirect на сторінку кошика
   - Або показати toast з опцією "Продовжити покупки"

6. **Download as Image (опціонально)**
   - Завантажити calendar як PNG
   - High resolution (300 DPI)
   - Для self-printing

## Файли для створення/модифікації

- `src/components/features/editor/PreviewModal.tsx`
- `src/components/features/editor/FormatSelector.tsx`
- `src/components/features/editor/EditorToolbar.tsx` - додати preview кнопку
- `src/services/cartService.ts`
- `src/types/cart.ts`

## Критерії успіху

- [ ] Preview modal відкривається
- [ ] Calendar відображається в high quality
- [ ] Можна вибрати формат (A3, A4)
- [ ] Можна вибрати paper type (Glossy, Matte)
- [ ] Ціна оновлюється залежно від вибору
- [ ] "Додати в кошик" працює
- [ ] Redirect на кошик або toast notification

## Залежності

- Task 12: Canvas повинен бути готовий
- Task 13, 14: Інструменти повинні працювати

## Блокується наступні задачі

- Task 17: Кошик потребує preview modal

## Технічні деталі

### src/components/features/editor/PreviewModal.tsx
```typescript
'use client'

import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { cartService } from '@/services/cartService'
import { useRouter } from 'next/navigation'

interface PreviewModalProps {
  isOpen: boolean
  onClose: () => void
  canvasDataURL: string
  calendarId: string
}

export default function PreviewModal({
  isOpen,
  onClose,
  canvasDataURL,
  calendarId,
}: PreviewModalProps) {
  const router = useRouter()
  const [format, setFormat] = useState<'A3' | 'A4'>('A4')
  const [paperType, setPaperType] = useState<'Glossy' | 'Matte'>('Glossy')
  const [quantity, setQuantity] = useState(1)

  const calculatePrice = () => {
    const basePrice = format === 'A3' ? 399 : 299
    const paperMultiplier = paperType === 'Glossy' ? 1.2 : 1.0
    return basePrice * paperMultiplier * quantity
  }

  const handleAddToCart = async () => {
    try {
      await cartService.addItem({
        calendarId,
        format,
        paperType,
        quantity,
        price: calculatePrice(),
      })

      router.push('/cart')
    } catch (error) {
      console.error('Failed to add to cart:', error)
    }
  }

  const handleDownload = () => {
    const link = document.createElement('a')
    link.download = `calendar-${calendarId}.png`
    link.href = canvasDataURL
    link.click()
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-6xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-2xl">Попередній перегляд календаря</DialogTitle>
        </DialogHeader>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Preview Image */}
          <div className="lg:col-span-2">
            <div className="border rounded-lg overflow-hidden bg-gray-100">
              <img
                src={canvasDataURL}
                alt="Calendar Preview"
                className="w-full h-auto"
              />
            </div>
          </div>

          {/* Options */}
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2">Формат</label>
              <Select value={format} onValueChange={(v) => setFormat(v as any)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="A4">A4 (210 x 297 мм)</SelectItem>
                  <SelectItem value="A3">A3 (297 x 420 мм)</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">Тип паперу</label>
              <Select value={paperType} onValueChange={(v) => setPaperType(v as any)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Glossy">Глянцевий (+20%)</SelectItem>
                  <SelectItem value="Matte">Матовий</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">Кількість</label>
              <Input
                type="number"
                min={1}
                max={100}
                value={quantity}
                onChange={(e) => setQuantity(parseInt(e.target.value) || 1)}
              />
            </div>

            <div className="border-t pt-4">
              <div className="flex justify-between mb-4">
                <span className="font-medium">Ціна:</span>
                <span className="text-2xl font-bold">{calculatePrice()} грн</span>
              </div>

              <Button
                onClick={handleAddToCart}
                className="w-full"
                size="lg"
              >
                Додати в кошик
              </Button>

              <Button
                variant="outline"
                onClick={handleDownload}
                className="w-full mt-2"
              >
                Завантажити як зображення
              </Button>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
```

### High-Quality Export
```typescript
// In CanvasEditor.tsx

const handlePreview = () => {
  if (!fabricCanvas) return

  // Generate high-quality preview
  const dataURL = fabricCanvas.toDataURL({
    format: 'png',
    quality: 1.0,
    multiplier: 3, // 3x for high DPI
  })

  setPreviewDataURL(dataURL)
  setIsPreviewOpen(true)
}
```

### src/services/cartService.ts
```typescript
import axios from 'axios'

const API_URL = process.env.NEXT_PUBLIC_API_URL

interface AddToCartRequest {
  calendarId: string
  format: 'A3' | 'A4'
  paperType: 'Glossy' | 'Matte'
  quantity: number
  price: number
}

export const cartService = {
  async addItem(data: AddToCartRequest): Promise<void> {
    await axios.post(`${API_URL}/cart/items`, data)
  },

  async getCart(): Promise<CartItem[]> {
    const response = await axios.get(`${API_URL}/cart/items`)
    return response.data
  },

  async removeItem(itemId: string): Promise<void> {
    await axios.delete(`${API_URL}/cart/items/${itemId}`)
  },
}
```

## Примітки

- multiplier: 3 для 300 DPI якості (потрібно для друку)
- Preview modal повинен показувати realistic size
- Ціна може змінюватися залежно від формату та паперу
- Додано `EditorPreviewDialogComponent` (Angular Material) з форматами, типами паперу та інтеграцією `CalendarService.addToCart`.

## Чому Codex?

Типова preview UI задача:
- Modal з опціями
- Form controls (select, input)
- Image display
- Price calculation (проста логіка)
- Інтеграція з cart API

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
