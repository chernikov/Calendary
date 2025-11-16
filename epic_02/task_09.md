# Task 09: Сторінка каталогу шаблонів

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 4-5 годин
**Відповідальний AI**: Codex
**Залежить від**: Task 03, 04
**Паралельно з**: Task 12, 13, 14

## Опис задачі

Створити сторінку каталогу шаблонів календарів з grid layout, картками шаблонів, lazy loading.

## Проблема

Користувачі повинні мати можливість переглядати всі доступні шаблони календарів в зручному форматі.

## Що треба зробити

1. **Створити сторінку каталогу**
   - `src/app/catalog/page.tsx`
   - Grid layout (3-4 колонки на desktop)
   - Responsive (1 колонка на mobile, 2 на tablet)

2. **Створити TemplateCard компонент**
   - `src/components/features/catalog/TemplateCard.tsx`
   - Preview image
   - Назва шаблону
   - Опис (truncated)
   - Ціна
   - Кнопка "Вибрати шаблон"
   - Hover effects

3. **Інтеграція з API**
   - `src/services/templateService.ts`
   - Fetch шаблонів з `/api/templates`
   - Loading state
   - Error handling
   - Empty state (якщо немає шаблонів)

4. **Додати Pagination або Infinite Scroll**
   - Load more при скролі (intersection observer)
   - Або pagination з кнопками
   - Показувати 12 шаблонів за раз

5. **Додати skeleton loaders**
   - Показувати під час завантаження
   - Використовувати shadcn/ui Skeleton

6. **Оптимізація зображень**
   - Використовувати Next.js Image component
   - Lazy loading
   - Blur placeholder

## Файли для створення/модифікації

- `src/app/catalog/page.tsx`
- `src/components/features/catalog/TemplateCard.tsx`
- `src/components/features/catalog/TemplateGrid.tsx`
- `src/components/features/catalog/TemplateCardSkeleton.tsx`
- `src/services/templateService.ts`
- `src/types/template.ts`

## Критерії успіху

- [ ] Каталог відображає всі активні шаблони
- [ ] Grid layout responsive на всіх екранах
- [ ] Images lazy loading працює
- [ ] Skeleton loaders показуються під час завантаження
- [ ] Клік на "Вибрати шаблон" переходить до редактора
- [ ] Error state відображається при помилці API

## Залежності

- Task 03: Layout повинен бути готовий
- Task 04: API для templates повинен працювати

## Блокується наступні задачі

- Task 10: Фільтри залежать від каталогу
- Task 11: Preview modal залежить від каталогу

## Технічні деталі

### src/app/catalog/page.tsx
```typescript
import { Suspense } from 'react'
import TemplateGrid from '@/components/features/catalog/TemplateGrid'
import TemplateCardSkeleton from '@/components/features/catalog/TemplateCardSkeleton'

export const metadata = {
  title: 'Каталог шаблонів - Calendary',
  description: 'Оберіть шаблон календаря та створіть свій унікальний дизайн',
}

export default function CatalogPage() {
  return (
    <div className="container mx-auto py-8">
      <h1 className="text-4xl font-bold mb-8">Каталог шаблонів</h1>

      <Suspense fallback={<TemplateGridSkeleton />}>
        <TemplateGrid />
      </Suspense>
    </div>
  )
}

function TemplateGridSkeleton() {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {Array.from({ length: 12 }).map((_, i) => (
        <TemplateCardSkeleton key={i} />
      ))}
    </div>
  )
}
```

### src/components/features/catalog/TemplateCard.tsx
```typescript
'use client'

import Image from 'next/image'
import Link from 'next/link'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Template } from '@/types/template'

interface TemplateCardProps {
  template: Template
}

export default function TemplateCard({ template }: TemplateCardProps) {
  return (
    <Card className="overflow-hidden hover:shadow-lg transition-shadow">
      <CardHeader className="p-0">
        <div className="relative aspect-[3/4] w-full">
          <Image
            src={template.previewImageUrl}
            alt={template.name}
            fill
            className="object-cover"
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 25vw"
          />
        </div>
      </CardHeader>

      <CardContent className="p-4">
        <CardTitle className="mb-2">{template.name}</CardTitle>
        <p className="text-sm text-gray-600 line-clamp-2">
          {template.description}
        </p>
      </CardContent>

      <CardFooter className="p-4 pt-0 flex justify-between items-center">
        <span className="text-xl font-bold">{template.price} грн</span>
        <Button asChild>
          <Link href={`/editor/new?template=${template.id}`}>
            Вибрати
          </Link>
        </Button>
      </CardFooter>
    </Card>
  )
}
```

### src/services/templateService.ts
```typescript
import axios from 'axios'
import { Template } from '@/types/template'

const API_URL = process.env.NEXT_PUBLIC_API_URL

export const templateService = {
  async getAll(): Promise<Template[]> {
    const response = await axios.get(`${API_URL}/templates`)
    return response.data
  },

  async getById(id: string): Promise<Template> {
    const response = await axios.get(`${API_URL}/templates/${id}`)
    return response.data
  },
}
```

## Примітки

- Next.js Image автоматично оптимізує зображення
- Skeleton loaders покращують UX під час завантаження
- line-clamp-2 обрізає опис до 2 рядків

## Чому Codex?

Типова UI задача:
- Стандартний grid layout
- CRUD list view
- Базові React компоненти
- Інтеграція з API без складної логіки

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
