# Task 02: Налаштувати UI Kit (Tailwind + shadcn/ui)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 3-4 години
**Відповідальний AI**: Codex
**Залежить від**: Task 01
**Паралельно з**: Task 04, 05, 06, 07, 08

## Опис задачі

Налаштувати Tailwind CSS та shadcn/ui компоненти для створення сучасного UI Kit. Створити дизайн-систему з кольорами, типографікою, та базовими компонентами.

## Проблема

Потрібна консистентна дизайн-система для всього Customer Portal з готовими UI компонентами.

## Що треба зробити

1. **Налаштувати Tailwind CSS**
   ```bash
   npm install -D tailwindcss postcss autoprefixer
   npx tailwindcss init -p
   ```
   - Налаштувати `tailwind.config.js`
   - Додати custom кольори, шрифти, spacing
   - Створити `src/styles/globals.css`

2. **Встановити shadcn/ui**
   ```bash
   npx shadcn-ui@latest init
   ```
   - Вибрати стиль (New York або Default)
   - Налаштувати theme colors
   - Встановити базові компоненти:
     - Button
     - Input
     - Card
     - Badge
     - Dialog
     - Dropdown Menu
     - Form
     - Label
     - Select
     - Tabs
     - Toast

3. **Створити дизайн-систему**
   - Визначити color palette:
     - Primary (бренд колір)
     - Secondary
     - Accent
     - Neutral (gray scale)
     - Success, Warning, Error
   - Налаштувати typography:
     - Headings (h1-h6)
     - Body text (sm, base, lg)
     - Font families
   - Налаштувати spacing та breakpoints

4. **Створити custom компоненти**
   - `src/components/ui/` - shadcn компоненти
   - `src/components/layout/Header.tsx`
   - `src/components/layout/Footer.tsx`
   - `src/components/layout/Container.tsx`

5. **Створити приклад Storybook сторінки**
   - Сторінка для демонстрації всіх компонентів
   - Допомагає перевірити консистентність UI

6. **Налаштувати темну тему (опціонально)**
   - CSS variables для light/dark theme
   - Theme switcher компонент

## Файли для створення/модифікації

- `tailwind.config.js`
- `postcss.config.js`
- `src/styles/globals.css`
- `src/components/ui/` - shadcn компоненти
- `src/components/layout/Header.tsx`
- `src/components/layout/Footer.tsx`
- `src/components/layout/Container.tsx`

## Критерії успіху

- [ ] Tailwind CSS налаштований та працює
- [ ] shadcn/ui компоненти встановлені
- [ ] Custom кольори та типографіка визначені
- [ ] Базові layout компоненти створені
- [ ] Все виглядає консистентно
- [ ] Responsive дизайн працює на всіх breakpoints

## Залежності

- Task 01: Next.js проект повинен бути створений

## Блокується наступні задачі

- Task 03: Layout залежить від UI компонентів
- Task 09: Каталог потребує UI компонентів

## Технічні деталі

### tailwind.config.js
```javascript
/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ["class"],
  content: [
    './pages/**/*.{ts,tsx}',
    './components/**/*.{ts,tsx}',
    './app/**/*.{ts,tsx}',
    './src/**/*.{ts,tsx}',
  ],
  theme: {
    container: {
      center: true,
      padding: "2rem",
      screens: {
        "2xl": "1400px",
      },
    },
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        // ... інші кольори
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [require("tailwindcss-animate")],
}
```

### globals.css
```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    --primary: 221.2 83.2% 53.3%;
    /* ... інші CSS variables */
  }
}
```

## Примітки

- shadcn/ui - це не бібліотека компонентів, а набір copy-paste компонентів
- Компоненти можна кастомізувати під проект
- Використовувати Radix UI під капотом для accessibility

## Чому Codex?

Типова setup задача:
- Стандартна конфігурація Tailwind
- Встановлення готових компонентів
- Шаблонна робота без складних рішень

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: 2025-11-16 (Codex)
