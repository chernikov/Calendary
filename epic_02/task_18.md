# Task 18: Checkout форма

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Codex
**Залежить від**: Task 17

## Опис задачі

Створити checkout сторінку з формою даних користувача (ім'я, телефон, email, адреса), вибором доставки, валідацією через React Hook Form + Zod.

## Проблема

Користувачі повинні мати можливість ввести свої дані для оформлення замовлення та доставки.

## Що треба зробити

1. **Створити checkout сторінку**
   - `src/app/checkout/page.tsx`
   - Multi-step form (опціонально):
     - Step 1: Контактні дані
     - Step 2: Доставка
     - Step 3: Оплата
   - Або single-page form

2. **Створити ContactForm компонент**
   - `src/components/features/checkout/ContactForm.tsx`
   - React Hook Form для керування формою
   - Zod для валідації
   - Поля:
     - Ім'я (обов'язкове)
     - Прізвище (обов'язкове)
     - Email (обов'язкове, email format)
     - Телефон (обов'язкове, phone format)

3. **Створити DeliveryForm компонент**
   - `src/components/features/checkout/DeliveryForm.tsx`
   - Вибір методу доставки:
     - Нова Пошта (відділення)
     - Нова Пошта (кур'єр)
     - Самовивіз (опціонально)
   - Для Нова Пошта:
     - Вибір міста (autocomplete)
     - Вибір відділення (dropdown)
   - Для кур'єра:
     - Адреса доставки (вулиця, будинок, квартира)

4. **Створити OrderSummary компонент**
   - `src/components/features/checkout/OrderSummary.tsx`
   - Список товарів (readonly)
   - Subtotal
   - Вартість доставки (з Nova Poshta API)
   - Total
   - Кнопка "Перейти до оплати"

5. **Form Validation (Zod)**
   - `src/lib/validations/checkout.ts`
   - Схеми валідації для кожного поля
   - Error messages українською

6. **Інтеграція з backend**
   - POST /api/orders - створити замовлення
   - Передати дані форми, cart items
   - Отримати orderId для payment

## Файли для створення/модифікації

- `src/app/checkout/page.tsx`
- `src/components/features/checkout/ContactForm.tsx`
- `src/components/features/checkout/DeliveryForm.tsx`
- `src/components/features/checkout/OrderSummary.tsx`
- `src/lib/validations/checkout.ts`
- `src/services/orderService.ts`
- `src/types/order.ts`

## Критерії успіху

- [ ] Форма відображається з всіма полями
- [ ] Валідація працює для всіх полів
- [ ] Error messages показуються українською
- [ ] Можна вибрати метод доставки
- [ ] Nova Poshta integration працює (вибір міста, відділення)
- [ ] Order summary показує правильні ціни
- [ ] "Перейти до оплати" створює замовлення
- [ ] Redirect на payment page (MonoBank)

## Залежності

- Task 17: Кошик повинен працювати

## Блокується наступні задачі

- Task 19: MonoBank інтеграція потребує checkout
- Task 20: Nova Poshta інтеграція потребує checkout

## Технічні деталі

### src/lib/validations/checkout.ts
```typescript
import { z } from 'zod'

export const contactFormSchema = z.object({
  firstName: z.string().min(2, "Ім'я має містити мінімум 2 символи"),
  lastName: z.string().min(2, "Прізвище має містити мінімум 2 символи"),
  email: z.string().email("Невірний формат email"),
  phone: z.string().regex(/^\+380\d{9}$/, "Невірний формат телефону (+380XXXXXXXXX)"),
})

export const deliveryFormSchema = z.object({
  method: z.enum(['nova-poshta-warehouse', 'nova-poshta-courier', 'pickup']),
  city: z.string().min(1, "Оберіть місто"),
  warehouse: z.string().optional(),
  address: z.object({
    street: z.string().optional(),
    building: z.string().optional(),
    apartment: z.string().optional(),
  }).optional(),
})

export type ContactFormData = z.infer<typeof contactFormSchema>
export type DeliveryFormData = z.infer<typeof deliveryFormSchema>
```

### src/app/checkout/page.tsx
```typescript
'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import ContactForm from '@/components/features/checkout/ContactForm'
import DeliveryForm from '@/components/features/checkout/DeliveryForm'
import OrderSummary from '@/components/features/checkout/OrderSummary'
import { contactFormSchema, deliveryFormSchema } from '@/lib/validations/checkout'
import { orderService } from '@/services/orderService'
import { useRouter } from 'next/navigation'

export default function CheckoutPage() {
  const router = useRouter()
  const [isSubmitting, setIsSubmitting] = useState(false)

  const contactForm = useForm({
    resolver: zodResolver(contactFormSchema),
  })

  const deliveryForm = useForm({
    resolver: zodResolver(deliveryFormSchema),
  })

  const handleSubmit = async () => {
    const contactValid = await contactForm.trigger()
    const deliveryValid = await deliveryForm.trigger()

    if (!contactValid || !deliveryValid) return

    try {
      setIsSubmitting(true)

      const order = await orderService.create({
        contact: contactForm.getValues(),
        delivery: deliveryForm.getValues(),
      })

      // Redirect to payment
      router.push(`/payment/${order.id}`)
    } catch (error) {
      console.error('Failed to create order:', error)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-8">Оформлення замовлення</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">
          <ContactForm form={contactForm} />
          <DeliveryForm form={deliveryForm} />
        </div>

        <div>
          <OrderSummary onSubmit={handleSubmit} isSubmitting={isSubmitting} />
        </div>
      </div>
    </div>
  )
}
```

### src/components/features/checkout/ContactForm.tsx
```typescript
'use client'

import { UseFormReturn } from 'react-hook-form'
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { ContactFormData } from '@/lib/validations/checkout'

interface ContactFormProps {
  form: UseFormReturn<ContactFormData>
}

export default function ContactForm({ form }: ContactFormProps) {
  const { register, formState: { errors } } = form

  return (
    <Card>
      <CardHeader>
        <CardTitle>Контактні дані</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label htmlFor="firstName">Ім'я *</Label>
            <Input
              id="firstName"
              {...register('firstName')}
              error={errors.firstName?.message}
            />
          </div>

          <div>
            <Label htmlFor="lastName">Прізвище *</Label>
            <Input
              id="lastName"
              {...register('lastName')}
              error={errors.lastName?.message}
            />
          </div>
        </div>

        <div>
          <Label htmlFor="email">Email *</Label>
          <Input
            id="email"
            type="email"
            {...register('email')}
            error={errors.email?.message}
          />
        </div>

        <div>
          <Label htmlFor="phone">Телефон *</Label>
          <Input
            id="phone"
            placeholder="+380XXXXXXXXX"
            {...register('phone')}
            error={errors.phone?.message}
          />
        </div>
      </CardContent>
    </Card>
  )
}
```

### src/components/features/checkout/DeliveryForm.tsx
```typescript
'use client'

import { useState } from 'react'
import { UseFormReturn } from 'react-hook-form'
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { DeliveryFormData } from '@/lib/validations/checkout'
import NovaPoshtaCitySearch from './NovaPoshtaCitySearch'
import NovaPoshtaWarehouseSelect from './NovaPoshtaWarehouseSelect'

interface DeliveryFormProps {
  form: UseFormReturn<DeliveryFormData>
}

export default function DeliveryForm({ form }: DeliveryFormProps) {
  const { register, watch, setValue } = form
  const deliveryMethod = watch('method')
  const selectedCity = watch('city')

  return (
    <Card>
      <CardHeader>
        <CardTitle>Доставка</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <RadioGroup
          value={deliveryMethod}
          onValueChange={(value) => setValue('method', value as any)}
        >
          <div className="flex items-center space-x-2">
            <RadioGroupItem value="nova-poshta-warehouse" id="warehouse" />
            <Label htmlFor="warehouse">Нова Пошта (у відділення)</Label>
          </div>

          <div className="flex items-center space-x-2">
            <RadioGroupItem value="nova-poshta-courier" id="courier" />
            <Label htmlFor="courier">Нова Пошта (кур'єр)</Label>
          </div>
        </RadioGroup>

        {deliveryMethod && deliveryMethod.startsWith('nova-poshta') && (
          <>
            <div>
              <Label>Місто</Label>
              <NovaPoshtaCitySearch
                onSelect={(city) => setValue('city', city)}
              />
            </div>

            {deliveryMethod === 'nova-poshta-warehouse' && selectedCity && (
              <div>
                <Label>Відділення</Label>
                <NovaPoshtaWarehouseSelect
                  city={selectedCity}
                  onSelect={(warehouse) => setValue('warehouse', warehouse)}
                />
              </div>
            )}
          </>
        )}
      </CardContent>
    </Card>
  )
}
```

## Примітки

- React Hook Form - популярна бібліотека для форм
- Zod - type-safe валідація
- Multi-step form покращує UX але складніший
- Nova Poshta інтеграція буде в Task 20

## Чому Codex?

Типова форма задача:
- React Hook Form з валідацією
- Стандартні form inputs
- Conditional rendering для delivery methods
- Базова бізнес-логіка

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
