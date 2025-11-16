# Task 03: Створити маршрутизацію та базовий Layout

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 3-4 години
**Відповідальний AI**: Codex
**Залежить від**: Task 02
**Паралельно з**: Task 04, 05, 06

## Опис задачі

Створити структуру маршрутизації Next.js App Router, Header, Footer, базовий Layout для всіх сторінок Customer Portal.

## Проблема

Потрібна навігаційна структура та консистентний layout для всіх сторінок порталу.

## Що треба зробити

1. **Створити структуру App Router**
   ```
   src/app/
   ├── layout.tsx           # Root layout
   ├── page.tsx             # Home page
   ├── catalog/
   │   └── page.tsx         # Каталог шаблонів
   ├── editor/
   │   └── [id]/
   │       └── page.tsx     # Редактор календаря
   ├── cart/
   │   └── page.tsx         # Кошик
   ├── checkout/
   │   └── page.tsx         # Оформлення замовлення
   ├── profile/
   │   ├── page.tsx         # Профіль користувача
   │   └── orders/
   │       └── page.tsx     # Історія замовлень
   └── auth/
       ├── login/
       │   └── page.tsx     # Логін
       └── register/
           └── page.tsx     # Реєстрація
   ```

2. **Створити Root Layout**
   - `src/app/layout.tsx`
   - Підключити fonts (Inter, Montserrat)
   - Підключити globals.css
   - Metadata для SEO
   - Analytics (Google Analytics, опціонально)

3. **Створити Header компонент**
   - `src/components/layout/Header.tsx`
   - Logo
   - Навігаційне меню (Каталог, Кошик)
   - User menu (Login/Register або Profile)
   - Search bar (опціонально)
   - Responsive mobile menu

4. **Створити Footer компонент**
   - `src/components/layout/Footer.tsx`
   - Посилання (Про нас, Контакти, Умови використання)
   - Соціальні мережі
   - Copyright

5. **Створити базові сторінки**
   - Home page з CTA кнопками
   - Placeholder сторінки для catalog, editor, cart
   - 404 сторінка (`not-found.tsx`)

6. **Налаштувати навігацію**
   - `src/lib/navigation.ts` - константи для routes
   - Використовувати Next.js `<Link>` компонент
   - Active link styling

## Файли для створення/модифікації

- `src/app/layout.tsx`
- `src/app/page.tsx`
- `src/app/not-found.tsx`
- `src/app/catalog/page.tsx`
- `src/app/editor/[id]/page.tsx`
- `src/app/cart/page.tsx`
- `src/app/checkout/page.tsx`
- `src/app/profile/page.tsx`
- `src/app/profile/orders/page.tsx`
- `src/app/auth/login/page.tsx`
- `src/app/auth/register/page.tsx`
- `src/components/layout/Header.tsx`
- `src/components/layout/Footer.tsx`
- `src/lib/navigation.ts`

## Критерії успіху

- [ ] Всі маршрути створені та доступні
- [ ] Header показується на всіх сторінках
- [ ] Footer показується на всіх сторінках
- [ ] Навігація між сторінками працює
- [ ] Mobile menu працює на маленьких екранах
- [ ] 404 сторінка відображається для неіснуючих маршрутів
- [ ] SEO metadata налаштована

## Залежності

- Task 02: UI компоненти повинні бути готові

## Блокується наступні задачі

- Task 09: Каталог потребує готовий layout
- Task 12: Редактор потребує готовий layout
- Task 17: Кошик потребує готовий layout

## Технічні деталі

### src/app/layout.tsx
```typescript
import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'
import Header from '@/components/layout/Header'
import Footer from '@/components/layout/Footer'

const inter = Inter({ subsets: ['latin', 'cyrillic'] })

export const metadata: Metadata = {
  title: 'Calendary - Створи свій унікальний календар',
  description: 'Платформа для створення персоналізованих календарів',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="uk">
      <body className={inter.className}>
        <Header />
        <main className="min-h-screen">{children}</main>
        <Footer />
      </body>
    </html>
  )
}
```

### src/lib/navigation.ts
```typescript
export const ROUTES = {
  HOME: '/',
  CATALOG: '/catalog',
  EDITOR: '/editor',
  CART: '/cart',
  CHECKOUT: '/checkout',
  PROFILE: '/profile',
  ORDERS: '/profile/orders',
  LOGIN: '/auth/login',
  REGISTER: '/auth/register',
} as const
```

## Примітки

- Next.js App Router використовує file-based routing
- Layout автоматично wraps всі дочірні сторінки
- Metadata важлива для SEO

## Чому Codex?

Стандартна задача з чіткою структурою:
- Типовий Next.js routing
- Базові React компоненти
- Навігація без складної логіки

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: 2025-11-16 (Codex)
