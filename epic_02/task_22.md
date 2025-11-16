# Task 22: Сторінка профілю користувача

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 4-5 годин
**Відповідальний AI**: Codex
**Залежить від**: Task 21
**Паралельно з**: Task 24, 25, 26

## Опис задачі

Створити сторінку профілю користувача з можливістю редагування особистих даних, зміни паролю, налаштувань.

## Що треба зробити

1. **Створити Profile сторінку**
   - `src/app/profile/page.tsx`
   - Tabs: Особисті дані, Змінити пароль, Налаштування
   - Protected route (потрібна авторизація)

2. **Personal Info Tab**
   - Форма редагування:
     - Ім'я
     - Прізвище
     - Email (readonly)
     - Телефон
     - Дата народження (опціонально)
   - Кнопка "Зберегти зміни"

3. **Change Password Tab**
   - Форма зміни паролю:
     - Поточний пароль
     - Новий пароль
     - Підтвердження нового паролю
   - Валідація: мінімум 8 символів, 1 велика, 1 цифра
   - Кнопка "Змінити пароль"

4. **Settings Tab (опціонально)**
   - Email notifications (toggle)
   - Language preference
   - Theme (light/dark)

5. **API Integration**
   - GET /api/users/me - отримати дані користувача
   - PUT /api/users/me - оновити дані
   - POST /api/users/change-password - змінити пароль

6. **Form Validation**
   - React Hook Form + Zod
   - Error messages
   - Success toast після збереження

## Файли для створення/модифікації

- `src/app/profile/page.tsx`
- `src/components/features/profile/PersonalInfoForm.tsx`
- `src/components/features/profile/ChangePasswordForm.tsx`
- `src/services/userService.ts`
- `src/lib/validations/profile.ts`

## Критерії успіху

- [ ] Profile сторінка показує дані користувача
- [ ] Можна редагувати особисті дані
- [ ] Можна змінити пароль
- [ ] Валідація працює для всіх полів
- [ ] Success/error messages показуються
- [ ] Дані оновлюються на backend

## Залежності

- Task 21: Auth повинна працювати

## Технічні деталі

### src/app/profile/page.tsx
```typescript
'use client'

import { useState } from 'react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import PersonalInfoForm from '@/components/features/profile/PersonalInfoForm'
import ChangePasswordForm from '@/components/features/profile/ChangePasswordForm'

export default function ProfilePage() {
  return (
    <div className="container mx-auto py-8 max-w-2xl">
      <h1 className="text-3xl font-bold mb-8">Мій профіль</h1>

      <Tabs defaultValue="personal">
        <TabsList>
          <TabsTrigger value="personal">Особисті дані</TabsTrigger>
          <TabsTrigger value="password">Змінити пароль</TabsTrigger>
        </TabsList>

        <TabsContent value="personal">
          <PersonalInfoForm />
        </TabsContent>

        <TabsContent value="password">
          <ChangePasswordForm />
        </TabsContent>
      </Tabs>
    </div>
  )
}
```

### src/components/features/profile/PersonalInfoForm.tsx
```typescript
'use client'

import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { personalInfoSchema } from '@/lib/validations/profile'
import { userService } from '@/services/userService'
import { toast } from 'sonner'

export default function PersonalInfoForm() {
  const { register, handleSubmit, formState: { errors }, reset } = useForm({
    resolver: zodResolver(personalInfoSchema),
  })

  useEffect(() => {
    // Load user data
    userService.getMe().then((user) => {
      reset(user)
    })
  }, [reset])

  const onSubmit = async (data: any) => {
    try {
      await userService.updateMe(data)
      toast.success('Дані оновлено!')
    } catch (error) {
      toast.error('Помилка оновлення')
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Особисті дані</CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label>Ім'я</Label>
              <Input {...register('firstName')} error={errors.firstName?.message} />
            </div>
            <div>
              <Label>Прізвище</Label>
              <Input {...register('lastName')} error={errors.lastName?.message} />
            </div>
          </div>

          <div>
            <Label>Email</Label>
            <Input {...register('email')} disabled />
          </div>

          <div>
            <Label>Телефон</Label>
            <Input {...register('phone')} error={errors.phone?.message} />
          </div>

          <Button type="submit">Зберегти зміни</Button>
        </form>
      </CardContent>
    </Card>
  )
}
```

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
