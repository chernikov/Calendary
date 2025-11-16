# Task 17: Кошик покупок

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Codex
**Залежить від**: Task 16

## Опис задачі

Створити сторінку кошика покупок з можливістю додавання/видалення товарів, зміни кількості, відображення загальної суми.

## Проблема

Користувачі повинні мати можливість переглянути свої обрані календарі перед оформленням замовлення.

## Що треба зробити

1. **Створити сторінку кошика**
   - `src/app/cart/page.tsx`
   - Список товарів в кошику
   - Empty state (коли кошик порожній)
   - Кнопка "Оформити замовлення"

2. **Створити CartItem компонент**
   - `src/components/features/cart/CartItem.tsx`
   - Preview image календаря
   - Назва календаря
   - Формат (A3/A4), Paper type
   - Ціна за одиницю
   - Quantity selector
   - Total price для item
   - Кнопка видалення

3. **Cart State Management (Zustand)**
   - `src/store/cartStore.ts`
   - addItem, removeItem, updateQuantity
   - getTotalPrice, getTotalItems
   - Sync з backend API

4. **Інтеграція з Backend API**
   - GET /api/cart/items - отримати кошик
   - POST /api/cart/items - додати item
   - PUT /api/cart/items/{id} - оновити кількість
   - DELETE /api/cart/items/{id} - видалити item

5. **Cart Summary компонент**
   - `src/components/features/cart/CartSummary.tsx`
   - Subtotal (сума товарів)
   - Доставка (розраховується пізніше)
   - Total
   - Кнопка "Продовжити до оплати"

6. **Cart Badge в Header**
   - Показувати кількість items в кошику
   - Update в real-time

## Файли для створення/модифікації

- `src/app/cart/page.tsx`
- `src/components/features/cart/CartItem.tsx`
- `src/components/features/cart/CartSummary.tsx`
- `src/components/features/cart/EmptyCart.tsx`
- `src/store/cartStore.ts`
- `src/services/cartService.ts`
- `src/components/layout/Header.tsx` - додати cart badge

## Критерії успіху

- [ ] Кошик відображає всі додані товари
- [ ] Можна змінити кількість товару
- [ ] Можна видалити товар з кошика
- [ ] Total price розраховується коректно
- [ ] Empty state показується коли кошик порожній
- [ ] Cart badge в header показує кількість items
- [ ] "Оформити замовлення" переходить до checkout

## Залежності

- Task 16: Preview modal для додавання в кошик

## Блокується наступні задачі

- Task 18: Checkout залежить від кошика

## Технічні деталі

### src/app/cart/page.tsx
```typescript
'use client'

import { useEffect } from 'react'
import { useCartStore } from '@/store/cartStore'
import CartItem from '@/components/features/cart/CartItem'
import CartSummary from '@/components/features/cart/CartSummary'
import EmptyCart from '@/components/features/cart/EmptyCart'
import { Button } from '@/components/ui/button'
import { useRouter } from 'next/navigation'

export default function CartPage() {
  const router = useRouter()
  const { items, fetchCart, removeItem, updateQuantity } = useCartStore()

  useEffect(() => {
    fetchCart()
  }, [fetchCart])

  if (items.length === 0) {
    return <EmptyCart />
  }

  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-8">Кошик</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Cart Items */}
        <div className="lg:col-span-2 space-y-4">
          {items.map((item) => (
            <CartItem
              key={item.id}
              item={item}
              onRemove={() => removeItem(item.id)}
              onUpdateQuantity={(qty) => updateQuantity(item.id, qty)}
            />
          ))}
        </div>

        {/* Cart Summary */}
        <div>
          <CartSummary />
        </div>
      </div>
    </div>
  )
}
```

### src/components/features/cart/CartItem.tsx
```typescript
'use client'

import Image from 'next/image'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Trash2 } from 'lucide-react'
import { CartItem as CartItemType } from '@/types/cart'

interface CartItemProps {
  item: CartItemType
  onRemove: () => void
  onUpdateQuantity: (quantity: number) => void
}

export default function CartItem({ item, onRemove, onUpdateQuantity }: CartItemProps) {
  return (
    <Card>
      <CardContent className="p-4">
        <div className="flex gap-4">
          {/* Preview Image */}
          <div className="relative w-24 h-32 flex-shrink-0">
            <Image
              src={item.calendar.previewImageUrl}
              alt={item.calendar.title}
              fill
              className="object-cover rounded"
            />
          </div>

          {/* Details */}
          <div className="flex-grow">
            <h3 className="font-semibold mb-1">{item.calendar.title}</h3>
            <p className="text-sm text-gray-600 mb-2">
              Формат: {item.format} | Папір: {item.paperType}
            </p>

            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <label className="text-sm">Кількість:</label>
                <Input
                  type="number"
                  min={1}
                  max={100}
                  value={item.quantity}
                  onChange={(e) => onUpdateQuantity(parseInt(e.target.value) || 1)}
                  className="w-20"
                />
              </div>

              <div className="ml-auto">
                <p className="text-sm text-gray-600">
                  {item.price} грн × {item.quantity}
                </p>
                <p className="text-lg font-bold">
                  {item.price * item.quantity} грн
                </p>
              </div>
            </div>
          </div>

          {/* Remove Button */}
          <Button
            variant="ghost"
            size="sm"
            onClick={onRemove}
            className="text-red-500"
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}
```

### src/components/features/cart/CartSummary.tsx
```typescript
'use client'

import { useRouter } from 'next/navigation'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useCartStore } from '@/store/cartStore'

export default function CartSummary() {
  const router = useRouter()
  const { getTotalPrice, getTotalItems } = useCartStore()

  const subtotal = getTotalPrice()
  const shipping = 0 // Буде розраховано в checkout
  const total = subtotal + shipping

  return (
    <Card className="sticky top-4">
      <CardHeader>
        <CardTitle>Разом</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex justify-between">
          <span>Товарів ({getTotalItems()}):</span>
          <span>{subtotal} грн</span>
        </div>

        <div className="flex justify-between">
          <span>Доставка:</span>
          <span className="text-gray-500">Розрахується пізніше</span>
        </div>

        <div className="border-t pt-4">
          <div className="flex justify-between text-lg font-bold">
            <span>До сплати:</span>
            <span>{total} грн</span>
          </div>
        </div>

        <Button
          className="w-full"
          size="lg"
          onClick={() => router.push('/checkout')}
        >
          Оформити замовлення
        </Button>

        <Button
          variant="outline"
          className="w-full"
          onClick={() => router.push('/catalog')}
        >
          Продовжити покупки
        </Button>
      </CardContent>
    </Card>
  )
}
```

### src/store/cartStore.ts
```typescript
import { create } from 'zustand'
import { cartService } from '@/services/cartService'
import { CartItem } from '@/types/cart'

interface CartStore {
  items: CartItem[]
  fetchCart: () => Promise<void>
  addItem: (item: Omit<CartItem, 'id'>) => Promise<void>
  removeItem: (itemId: string) => Promise<void>
  updateQuantity: (itemId: string, quantity: number) => Promise<void>
  getTotalPrice: () => number
  getTotalItems: () => number
}

export const useCartStore = create<CartStore>((set, get) => ({
  items: [],

  fetchCart: async () => {
    const items = await cartService.getCart()
    set({ items })
  },

  addItem: async (item) => {
    await cartService.addItem(item)
    await get().fetchCart()
  },

  removeItem: async (itemId) => {
    await cartService.removeItem(itemId)
    set((state) => ({
      items: state.items.filter((item) => item.id !== itemId),
    }))
  },

  updateQuantity: async (itemId, quantity) => {
    await cartService.updateQuantity(itemId, quantity)
    set((state) => ({
      items: state.items.map((item) =>
        item.id === itemId ? { ...item, quantity } : item
      ),
    }))
  },

  getTotalPrice: () => {
    return get().items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  },

  getTotalItems: () => {
    return get().items.reduce((sum, item) => sum + item.quantity, 0)
  },
}))
```

## Примітки

- Zustand - легка альтернатива Redux
- Cart state синхронізується з backend
- Cart badge покращує UX
- Empty state важливий для першого візиту

## Чому Codex?

Типова e-commerce cart задача:
- Стандартний CRUD UI
- List view з items
- State management (Zustand - простий)
- Базові розрахунки (ціна, кількість)

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
