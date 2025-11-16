# Task 29: Mobile responsive тестування

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Codex
**Залежить від**: Всі попередні

## Опис задачі

Протестувати та виправити responsive дизайн на різних девайсах, виправити UI bugs на мобільних, забезпечити mobile-first підхід.

## Проблема

Більшість користувачів можуть заходити з мобільних пристроїв, тому потрібно забезпечити ідеальний UX на всіх розмірах екранів.

## Що треба зробити

1. **Тестування на різних пристроях**
   - iPhone (Safari)
   - Android (Chrome)
   - Tablet (iPad, Android tablet)
   - Desktop (різні розміри)
   - Використовувати Chrome DevTools, BrowserStack

2. **Breakpoints перевірка**
   - Mobile: 320px - 640px
   - Tablet: 641px - 1024px
   - Desktop: 1025px+
   - Перевірити всі сторінки на всіх breakpoints

3. **Mobile Navigation**
   - Hamburger menu працює
   - Touch-friendly розміри (min 44x44px)
   - Swipe gestures (де потрібно)
   - Bottom navigation (опціонально)

4. **Canvas Editor на Mobile**
   - Touch controls для canvas
   - Pinch-to-zoom
   - Toolbar адаптований
   - Image upload з камери
   - Horizontal scroll (якщо потрібно)

5. **Forms на Mobile**
   - Input types правильні (email, tel, number)
   - Autocomplete працює
   - Keyboard не перекриває inputs
   - Submit buttons видимі

6. **Images та Media**
   - Responsive images
   - Правильні aspect ratios
   - Не overflow екрану
   - Lazy loading працює

7. **Performance на Mobile**
   - Швидке завантаження на 3G
   - Smooth scrolling
   - No janky animations
   - Battery-friendly

8. **Виправити знайдені bugs**
   - Horizontal scroll
   - Overlapping elements
   - Text too small
   - Buttons not clickable
   - Images не відображаються

## Файли для створення/модифікації

- `src/components/layout/MobileNav.tsx`
- `src/components/layout/Header.tsx` - mobile version
- `src/components/features/editor/CanvasEditor.tsx` - touch support
- CSS/Tailwind classes - responsive fixes
- `src/app/globals.css` - mobile-specific styles

## Критерії успіху

- [ ] Всі сторінки responsive на mobile
- [ ] Navigation працює на mobile
- [ ] Forms зручні на mobile
- [ ] Canvas editor працює з touch
- [ ] No horizontal scroll
- [ ] Text readable на малих екранах
- [ ] Buttons touch-friendly (min 44x44px)
- [ ] Images не overflow

## Залежності

- Всі features повинні бути імплементовані

## Технічні деталі

### MobileNav Component
```typescript
'use client'

import { useState } from 'react'
import { Menu, X } from 'lucide-react'
import Link from 'next/link'

export default function MobileNav() {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="md:hidden p-2"
        aria-label="Toggle menu"
      >
        {isOpen ? <X size={24} /> : <Menu size={24} />}
      </button>

      {isOpen && (
        <div className="fixed inset-0 top-16 bg-white z-50 md:hidden">
          <nav className="flex flex-col p-4 space-y-4">
            <Link
              href="/catalog"
              onClick={() => setIsOpen(false)}
              className="text-lg py-3 border-b"
            >
              Каталог
            </Link>
            <Link
              href="/cart"
              onClick={() => setIsOpen(false)}
              className="text-lg py-3 border-b"
            >
              Кошик
            </Link>
            <Link
              href="/profile"
              onClick={() => setIsOpen(false)}
              className="text-lg py-3 border-b"
            >
              Профіль
            </Link>
          </nav>
        </div>
      )}
    </>
  )
}
```

### Touch Support для Canvas
```typescript
// In CanvasEditor.tsx

useEffect(() => {
  if (!fabricCanvas) return

  // Enable touch scrolling
  fabricCanvas.allowTouchScrolling = true

  // Touch event handlers
  let lastTouchDistance = 0

  const handleTouchStart = (e: fabric.IEvent) => {
    const touch = e.e as TouchEvent
    if (touch.touches.length === 2) {
      const distance = getTouchDistance(touch.touches)
      lastTouchDistance = distance
    }
  }

  const handleTouchMove = (e: fabric.IEvent) => {
    const touch = e.e as TouchEvent
    if (touch.touches.length === 2) {
      // Pinch to zoom
      const distance = getTouchDistance(touch.touches)
      const scale = distance / lastTouchDistance

      fabricCanvas.zoomToPoint(
        { x: touch.touches[0].clientX, y: touch.touches[0].clientY },
        fabricCanvas.getZoom() * scale
      )

      lastTouchDistance = distance
    }
  }

  fabricCanvas.on('touch:gesture', handleTouchStart)
  fabricCanvas.on('touch:drag', handleTouchMove)

  return () => {
    fabricCanvas.off('touch:gesture', handleTouchStart)
    fabricCanvas.off('touch:drag', handleTouchMove)
  }
}, [fabricCanvas])

function getTouchDistance(touches: TouchList): number {
  const dx = touches[0].clientX - touches[1].clientX
  const dy = touches[0].clientY - touches[1].clientY
  return Math.sqrt(dx * dx + dy * dy)
}
```

### Mobile-First CSS
```css
/* globals.css */

/* Mobile first (default) */
.container {
  padding: 1rem;
}

.grid {
  grid-template-columns: 1fr;
  gap: 1rem;
}

/* Tablet */
@media (min-width: 768px) {
  .container {
    padding: 2rem;
  }

  .grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 1.5rem;
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .container {
    padding: 3rem;
  }

  .grid {
    grid-template-columns: repeat(3, 1fr);
    gap: 2rem;
  }
}

/* Prevent horizontal scroll */
body {
  overflow-x: hidden;
}

/* Touch-friendly buttons */
button,
a {
  min-height: 44px;
  min-width: 44px;
}
```

### Responsive Tailwind Classes
```typescript
// Use Tailwind responsive prefixes

<div className="
  flex flex-col           // Mobile: column
  md:flex-row             // Tablet+: row
  gap-4                   // Mobile: 1rem gap
  md:gap-6                // Tablet+: 1.5rem gap
  p-4                     // Mobile: 1rem padding
  md:p-6                  // Tablet+: 1.5rem padding
  lg:p-8                  // Desktop: 2rem padding
">
  <div className="
    w-full                // Mobile: full width
    md:w-1/2              // Tablet+: half width
    lg:w-1/3              // Desktop: third width
  ">
    Content
  </div>
</div>
```

### Mobile Form Optimization
```typescript
<form>
  <input
    type="email"          // Shows email keyboard
    inputMode="email"
    autoComplete="email"
  />

  <input
    type="tel"            // Shows phone keyboard
    inputMode="tel"
    autoComplete="tel"
    pattern="[0-9]*"      // Numeric keyboard on iOS
  />

  <input
    type="number"
    inputMode="numeric"
  />
</form>
```

## Примітки

- Mobile-first підхід: спочатку mobile, потім tablet/desktop
- Touch targets мінімум 44x44px (Apple HIG)
- Horizontal scroll - найчастіша проблема
- Test на реальних девайсах, не тільки емулятор

## Чому Codex?

UI/CSS testing задача:
- Responsive design fixes
- CSS debugging
- Tailwind classes
- Mobile navigation patterns
- Standard UX improvements

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
