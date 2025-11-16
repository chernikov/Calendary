# Task 23: Історія замовлень та статуси

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Codex
**Залежить від**: Task 21, 19, 20
**Паралельно з**: Task 24, 25, 26

## Опис задачі

Створити сторінку історії замовлень користувача з деталями, статусами, відстеженням ТТН Нової Пошти.

## Що треба зробити

1. **Створити Orders сторінку**
   - `src/app/profile/orders/page.tsx`
   - Список всіх замовлень користувача
   - Сортування (нові спочатку)
   - Пагінація або infinite scroll

2. **OrderCard компонент**
   - `src/components/features/orders/OrderCard.tsx`
   - Номер замовлення
   - Дата замовлення
   - Статус (Pending, Paid, Processing, Shipped, Delivered, Cancelled)
   - Загальна сума
   - Кнопка "Деталі"

3. **Order Details Modal/Page**
   - Детальна інформація про замовлення:
     - Список товарів (календарі)
     - Preview images
     - Кількість, формат, ціна
   - Інформація про доставку:
     - Адреса
     - Tracking number (ТТН)
     - Статус доставки
   - Payment info:
     - Метод оплати
     - Дата оплати

4. **Tracking Nova Poshta**
   - Якщо є ТТН - показувати статус доставки
   - Інтеграція з Nova Poshta tracking API
   - Timeline доставки (прийнято, в дорозі, доставлено)

5. **Order Status Colors**
   - Pending - жовтий
   - Paid - синій
   - Processing - помаранчевий
   - Shipped - фіолетовий
   - Delivered - зелений
   - Cancelled - червоний

6. **API Integration**
   - GET /api/orders - список замовлень користувача
   - GET /api/orders/{id} - деталі замовлення
   - GET /api/delivery/track/{ttn} - tracking info

## Файли для створення/модифікації

- `src/app/profile/orders/page.tsx`
- `src/app/profile/orders/[id]/page.tsx` - деталі
- `src/components/features/orders/OrderCard.tsx`
- `src/components/features/orders/OrderDetails.tsx`
- `src/components/features/orders/TrackingTimeline.tsx`
- `src/services/orderService.ts`

## Критерії успіху

- [ ] Показуються всі замовлення користувача
- [ ] Статуси відображаються коректно
- [ ] Можна переглянути деталі замовлення
- [ ] Tracking працює для відправлених замовлень
- [ ] Timeline доставки показується
- [ ] Order history empty state (якщо немає замовлень)

## Залежності

- Task 21: Auth
- Task 19: MonoBank (payment)
- Task 20: Nova Poshta (tracking)

## Технічні деталі

### src/app/profile/orders/page.tsx
```typescript
'use client'

import { useEffect, useState } from 'react'
import { orderService } from '@/services/orderService'
import OrderCard from '@/components/features/orders/OrderCard'

export default function OrdersPage() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    orderService.getMyOrders().then((data) => {
      setOrders(data)
      setLoading(false)
    })
  }, [])

  if (loading) return <div>Завантаження...</div>

  if (orders.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">У вас поки немає замовлень</p>
        <Button onClick={() => router.push('/catalog')}>
          Перейти до каталогу
        </Button>
      </div>
    )
  }

  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-8">Мої замовлення</h1>

      <div className="space-y-4">
        {orders.map((order) => (
          <OrderCard key={order.id} order={order} />
        ))}
      </div>
    </div>
  )
}
```

### src/components/features/orders/OrderCard.tsx
```typescript
'use client'

import Link from 'next/link'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { formatDate } from '@/lib/utils'

const STATUS_CONFIG = {
  Pending: { label: 'Очікує оплати', color: 'yellow' },
  Paid: { label: 'Оплачено', color: 'blue' },
  Processing: { label: 'В обробці', color: 'orange' },
  Shipped: { label: 'Відправлено', color: 'purple' },
  Delivered: { label: 'Доставлено', color: 'green' },
  Cancelled: { label: 'Скасовано', color: 'red' },
}

export default function OrderCard({ order }) {
  const status = STATUS_CONFIG[order.status]

  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex justify-between items-start">
          <div>
            <div className="flex items-center gap-3 mb-2">
              <h3 className="font-semibold">Замовлення #{order.orderNumber}</h3>
              <Badge variant={status.color}>{status.label}</Badge>
            </div>
            <p className="text-sm text-gray-600">
              {formatDate(order.createdAt)} • {order.itemsCount} товарів • {order.total} грн
            </p>
            {order.trackingNumber && (
              <p className="text-sm text-gray-600 mt-1">
                ТТН: {order.trackingNumber}
              </p>
            )}
          </div>

          <Button asChild variant="outline">
            <Link href={`/profile/orders/${order.id}`}>
              Деталі
            </Link>
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}
```

### src/components/features/orders/TrackingTimeline.tsx
```typescript
'use client'

import { useEffect, useState } from 'react'
import { deliveryService } from '@/services/deliveryService'

export default function TrackingTimeline({ trackingNumber }) {
  const [tracking, setTracking] = useState(null)

  useEffect(() => {
    if (trackingNumber) {
      deliveryService.track(trackingNumber).then(setTracking)
    }
  }, [trackingNumber])

  if (!tracking) return null

  return (
    <div className="space-y-4">
      <h3 className="font-semibold">Відстеження посилки</h3>

      <div className="relative">
        {tracking.events.map((event, index) => (
          <div key={index} className="flex gap-4 pb-4">
            <div className="flex flex-col items-center">
              <div className="w-3 h-3 rounded-full bg-blue-500" />
              {index < tracking.events.length - 1 && (
                <div className="w-0.5 h-full bg-gray-300 mt-1" />
              )}
            </div>
            <div>
              <p className="font-medium">{event.status}</p>
              <p className="text-sm text-gray-600">{event.date}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
```

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
