# Task 11: Попередній перегляд шаблону (modal)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: ✅ DONE (Calendary.Ng)
**Пріоритет**: P1 (Високий)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Codex
**Залежить від**: Task 09
**Паралельно з**: Task 12, 13, 14

## Опис задачі

Створити modal з детальним переглядом шаблону календаря, великим preview, описом, кнопкою "Створити календар".

## Проблема

Користувачі повинні мати можливість детально розглянути шаблон перед тим, як почати створювати календар.

## Що треба зробити

1. **Створити TemplatePreviewModal компонент**
   - `src/components/features/catalog/TemplatePreviewModal.tsx`
   - Використовувати shadcn/ui Dialog
   - Великий preview image
   - Детальний опис
   - Характеристики шаблону
   - Кнопка "Створити календар"

2. **Додати Image Gallery**
   - Показувати кілька зображень шаблону (якщо є)
   - Image carousel або grid
   - Можливість збільшити зображення

3. **Відображати детальну інформацію**
   - Назва шаблону
   - Повний опис
   - Категорія
   - Ціна
   - Розмір (A3, A4)
   - Рекомендації використання

4. **Інтеграція з каталогом**
   - Клік на картку шаблону відкриває modal
   - URL parameter для deep linking (?preview=template-id)
   - Можливість закрити ESC або кнопкою

5. **Додати кнопку "Створити календар"**
   - Редірект на /editor/new?template={id}
   - Початковий стан з шаблону

## Файли для створення/модифікації

- `src/components/features/catalog/TemplatePreviewModal.tsx`
- `src/components/features/catalog/TemplateCard.tsx` - додати onClick для відкриття modal
- `src/app/catalog/page.tsx` - інтегрувати modal

## Критерії успіху

- [ ] Modal відкривається при кліку на шаблон
- [ ] Велике зображення відображається
- [ ] Вся інформація про шаблон показується
- [ ] "Створити календар" переходить до редактора
- [ ] Modal закривається ESC або кліком поза ним
- [ ] URL оновлюється при відкритті (?preview=id)
- [ ] Deep linking працює (можна поділитися посиланням)

## Залежності

- Task 09: Каталог повинен бути готовий

## Блокується наступні задачі

Немає

## Технічні деталі

### src/components/features/catalog/TemplatePreviewModal.tsx
```typescript
'use client'

import { useRouter, useSearchParams } from 'next/navigation'
import Image from 'next/image'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Template } from '@/types/template'

interface TemplatePreviewModalProps {
  template: Template | null
  isOpen: boolean
  onClose: () => void
}

export default function TemplatePreviewModal({
  template,
  isOpen,
  onClose,
}: TemplatePreviewModalProps) {
  const router = useRouter()

  if (!template) return null

  const handleCreateCalendar = () => {
    router.push(`/editor/new?template=${template.id}`)
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-2xl">{template.name}</DialogTitle>
          <DialogDescription>
            <Badge>{template.category}</Badge>
          </DialogDescription>
        </DialogHeader>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Preview Image */}
          <div className="relative aspect-[3/4] w-full rounded-lg overflow-hidden">
            <Image
              src={template.previewImageUrl}
              alt={template.name}
              fill
              className="object-cover"
              sizes="(max-width: 768px) 100vw, 50vw"
              priority
            />
          </div>

          {/* Details */}
          <div className="space-y-4">
            <div>
              <h3 className="font-semibold mb-2">Опис</h3>
              <p className="text-gray-600">{template.description}</p>
            </div>

            <div>
              <h3 className="font-semibold mb-2">Характеристики</h3>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>• Категорія: {template.category}</li>
                <li>• Формат: A3, A4</li>
                <li>• Якість друку: 300 DPI</li>
                <li>• Можливість редагування: Так</li>
              </ul>
            </div>

            <div className="border-t pt-4">
              <div className="flex items-center justify-between mb-4">
                <span className="text-3xl font-bold">{template.price} грн</span>
              </div>

              <Button
                onClick={handleCreateCalendar}
                className="w-full"
                size="lg"
              >
                Створити календар з цього шаблону
              </Button>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
```

### Updated TemplateCard.tsx
```typescript
'use client'

import { useState } from 'react'
import TemplatePreviewModal from './TemplatePreviewModal'

export default function TemplateCard({ template }: TemplateCardProps) {
  const [isPreviewOpen, setIsPreviewOpen] = useState(false)

  return (
    <>
      <Card
        className="overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
        onClick={() => setIsPreviewOpen(true)}
      >
        {/* ... card content ... */}
      </Card>

      <TemplatePreviewModal
        template={template}
        isOpen={isPreviewOpen}
        onClose={() => setIsPreviewOpen(false)}
      />
    </>
  )
}
```

## Примітки

- shadcn/ui Dialog має вбудовану accessibility (ESC, focus trap)
- Deep linking дозволяє ділитися конкретним шаблоном
- Priority на Image в modal для швидкого завантаження
- На Angular-сторінці використано `MatDialog` та `TemplatePreviewDialogComponent` з підтримкою `?preview=` параметра.

## Чому Codex?

Типовий modal компонент:
- Стандартний Dialog UI
- Відображення деталей
- Проста навігація
- Базові React patterns

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
