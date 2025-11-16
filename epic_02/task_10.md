# Task 10: Фільтри та пошук в каталозі

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Низька
**Час**: 3-4 години
**Відповідальний AI**: Codex
**Залежить від**: Task 09
**Паралельно з**: Task 12, 13, 14

## Опис задачі

Додати фільтри по категоріях та ціні, пошук по ключовим словам в каталозі шаблонів.

## Проблема

Користувачі повинні мати можливість швидко знайти потрібний шаблон серед великої кількості варіантів.

## Що треба зробити

1. **Створити FilterBar компонент**
   - `src/components/features/catalog/FilterBar.tsx`
   - Фільтр по категоріях (dropdown)
   - Фільтр по ціні (range slider)
   - Пошук по назві (input)
   - Кнопка "Скинути фільтри"

2. **Додати category filter**
   - Отримати список категорій з API
   - Dropdown з категоріями
   - Multiple select або single select
   - Відображати кількість шаблонів в категорії

3. **Додати price range filter**
   - Slider з min/max ціною
   - Використовувати shadcn/ui Slider
   - Показувати поточний діапазон

4. **Додати search input**
   - Debounced input (500ms)
   - Пошук по назві та опису
   - Clear button

5. **Інтеграція з API**
   - Передавати фільтри в query params
   - GET /api/templates?category=Family&minPrice=100&maxPrice=500&search=christmas
   - Оновлювати URL (Next.js useSearchParams)

6. **Додати URL state**
   - Фільтри в URL query params
   - Зберігати стан при перезавантаженні
   - Share-able links

## Файли для створення/модифікації

- `src/components/features/catalog/FilterBar.tsx`
- `src/components/features/catalog/CategoryFilter.tsx`
- `src/components/features/catalog/PriceRangeFilter.tsx`
- `src/components/features/catalog/SearchInput.tsx`
- `src/app/catalog/page.tsx` - додати FilterBar
- `src/services/templateService.ts` - додати параметри фільтрів

## Критерії успіху

- [ ] Фільтр по категоріях працює
- [ ] Фільтр по ціні працює
- [ ] Пошук по назві працює (debounced)
- [ ] Фільтри зберігаються в URL
- [ ] "Скинути фільтри" очищає всі фільтри
- [ ] Результати оновлюються автоматично
- [ ] Показується кількість знайдених шаблонів

## Залежності

- Task 09: Каталог повинен бути готовий

## Блокується наступні задачі

Немає

## Технічні деталі

### src/components/features/catalog/FilterBar.tsx
```typescript
'use client'

import { useSearchParams, useRouter } from 'next/navigation'
import { useState, useCallback } from 'react'
import debounce from 'lodash/debounce'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Slider } from '@/components/ui/slider'
import { Button } from '@/components/ui/button'

const CATEGORIES = [
  { value: 'all', label: 'Всі категорії' },
  { value: 'family', label: 'Сімейні' },
  { value: 'corporate', label: 'Корпоративні' },
  { value: 'sport', label: 'Спортивні' },
  { value: 'kids', label: 'Дитячі' },
]

export default function FilterBar() {
  const router = useRouter()
  const searchParams = useSearchParams()

  const [search, setSearch] = useState(searchParams.get('search') || '')
  const [category, setCategory] = useState(searchParams.get('category') || 'all')
  const [priceRange, setPriceRange] = useState([
    parseInt(searchParams.get('minPrice') || '0'),
    parseInt(searchParams.get('maxPrice') || '1000'),
  ])

  const updateURL = useCallback((params: Record<string, string>) => {
    const current = new URLSearchParams(searchParams.toString())

    Object.entries(params).forEach(([key, value]) => {
      if (value) {
        current.set(key, value)
      } else {
        current.delete(key)
      }
    })

    router.push(`/catalog?${current.toString()}`)
  }, [searchParams, router])

  const debouncedSearch = useCallback(
    debounce((value: string) => {
      updateURL({ search: value })
    }, 500),
    [updateURL]
  )

  const handleSearchChange = (value: string) => {
    setSearch(value)
    debouncedSearch(value)
  }

  const handleCategoryChange = (value: string) => {
    setCategory(value)
    updateURL({ category: value === 'all' ? '' : value })
  }

  const handlePriceChange = (values: number[]) => {
    setPriceRange(values)
    updateURL({
      minPrice: values[0].toString(),
      maxPrice: values[1].toString(),
    })
  }

  const handleReset = () => {
    setSearch('')
    setCategory('all')
    setPriceRange([0, 1000])
    router.push('/catalog')
  }

  return (
    <div className="bg-white p-6 rounded-lg shadow mb-8">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {/* Search */}
        <Input
          placeholder="Пошук шаблонів..."
          value={search}
          onChange={(e) => handleSearchChange(e.target.value)}
        />

        {/* Category */}
        <Select value={category} onValueChange={handleCategoryChange}>
          <SelectTrigger>
            <SelectValue placeholder="Категорія" />
          </SelectTrigger>
          <SelectContent>
            {CATEGORIES.map((cat) => (
              <SelectItem key={cat.value} value={cat.value}>
                {cat.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        {/* Price Range */}
        <div className="col-span-1 md:col-span-1">
          <label className="text-sm text-gray-600 mb-2 block">
            Ціна: {priceRange[0]} - {priceRange[1]} грн
          </label>
          <Slider
            min={0}
            max={1000}
            step={50}
            value={priceRange}
            onValueChange={handlePriceChange}
          />
        </div>

        {/* Reset */}
        <Button variant="outline" onClick={handleReset}>
          Скинути фільтри
        </Button>
      </div>
    </div>
  )
}
```

### Updated templateService.ts
```typescript
interface GetTemplatesParams {
  category?: string
  minPrice?: number
  maxPrice?: number
  search?: string
}

export const templateService = {
  async getAll(params?: GetTemplatesParams): Promise<Template[]> {
    const queryParams = new URLSearchParams()

    if (params?.category) queryParams.append('category', params.category)
    if (params?.minPrice) queryParams.append('minPrice', params.minPrice.toString())
    if (params?.maxPrice) queryParams.append('maxPrice', params.maxPrice.toString())
    if (params?.search) queryParams.append('search', params.search)

    const response = await axios.get(
      `${API_URL}/templates?${queryParams.toString()}`
    )
    return response.data
  },
}
```

## Примітки

- Debounce важливий для search input (запобігає занадто багатьом API calls)
- URL state дозволяє ділитися посиланнями з фільтрами
- lodash/debounce - корисна утиліта

## Чому Codex?

Стандартна filter UI задача:
- Form controls (input, select, slider)
- URL state management
- Debouncing
- Типовий UX pattern

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
