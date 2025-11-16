# Task 01: Створити Next.js проект з TypeScript

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Codex
**Паралельно з**: Task 04, 05, 06, 07, 08

## Опис задачі

Ініціалізувати новий Next.js 14+ проект з TypeScript для Customer Portal. Налаштувати базову конфігурацію, ESLint, Prettier, та структуру проекту.

## Проблема

Потрібна сучасна frontend основа для Customer Portal з підтримкою TypeScript, SSR/SSG, та оптимізованої продуктивності.

## Що треба зробити

1. **Створити Next.js проект**
   ```bash
   npx create-next-app@latest calendary-customer-portal --typescript
   ```
   - App Router (не Pages Router)
   - TypeScript
   - ESLint
   - Tailwind CSS (буде налаштовано в Task 02)
   - `src/` directory
   - NO Turbopack (використовувати стандартний webpack)

2. **Налаштувати package.json**
   - Додати scripts: `dev`, `build`, `start`, `lint`, `type-check`
   - Встановити базові залежності:
     - `axios` - HTTP клієнт
     - `zustand` - state management
     - `react-hook-form` - форми
     - `zod` - валідація
     - `date-fns` - робота з датами
     - `clsx` - утиліта для className

3. **Налаштувати TypeScript**
   - `tsconfig.json` з strict mode
   - Path aliases: `@/` → `./src/`
   - Типи для API responses
   - Створити `src/types/` директорію

4. **Налаштувати ESLint**
   - `.eslintrc.json` з рекомендованими правилами Next.js
   - Додати правила для TypeScript
   - Додати правила для React Hooks

5. **Налаштувати Prettier**
   - `.prettierrc` з правилами форматування
   - Інтеграція з ESLint

6. **Створити структуру проекту**
   ```
   src/
   ├── app/              # Next.js App Router
   ├── components/       # React компоненти
   │   ├── ui/          # shadcn/ui компоненти (Task 02)
   │   ├── layout/      # Layout компоненти
   │   └── features/    # Feature-specific компоненти
   ├── lib/             # Утиліти, helpers
   ├── hooks/           # Custom React hooks
   ├── services/        # API services
   ├── store/           # Zustand stores
   ├── types/           # TypeScript types/interfaces
   └── styles/          # Global styles
   ```

7. **Створити базові конфігураційні файли**
   - `next.config.js` з налаштуваннями:
     - Image optimization
     - Redirects (якщо потрібно)
     - Environment variables
   - `.env.local.example` - приклад environment variables
   - `.gitignore` - ігнорувати node_modules, .next, .env.local

8. **Перевірити що все працює**
   - `npm run dev` - запустити dev сервер
   - Перевірити http://localhost:3000
   - `npm run build` - перевірити production build
   - `npm run lint` - перевірити ESLint

## Файли для створення/модифікації

- `calendary-customer-portal/` - новий проект
- `package.json`
- `tsconfig.json`
- `.eslintrc.json`
- `.prettierrc`
- `next.config.js`
- `.env.local.example`
- `src/` структура

## Критерії успіху

- [x] Next.js проект створений з TypeScript
- [x] `npm run dev` запускає dev сервер без помилок
- [x] `npm run build` успішно створює production build
- [x] `npm run lint` проходить без помилок
- [x] TypeScript strict mode увімкнений
- [x] Path aliases працюють (@/ → src/)
- [x] Структура проекту створена
- [x] README.md з інструкціями запуску

## Залежності

Немає (це перша задача Frontend треку)

## Блокується наступні задачі

- Task 02: UI Kit setup (залежить від цієї задачі)
- Task 03: Routing та Layout (залежить від цієї задачі)

## Технічні деталі

### next.config.js приклад
```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    domains: ['localhost', 'storage.googleapis.com'], // додати домени для зображень
  },
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
  },
}

module.exports = nextConfig
```

### .env.local.example
```
NEXT_PUBLIC_API_URL=http://localhost:5196/api
NEXT_PUBLIC_SITE_URL=http://localhost:3000
```

### tsconfig.json
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["dom", "dom.iterable", "esnext"],
    "allowJs": true,
    "skipLibCheck": true,
    "strict": true,
    "noEmit": true,
    "esModuleInterop": true,
    "module": "esnext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "jsx": "preserve",
    "incremental": true,
    "plugins": [
      {
        "name": "next"
      }
    ],
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["next-env.d.ts", "**/*.ts", "**/*.tsx", ".next/types/**/*.ts"],
  "exclude": ["node_modules"]
}
```

## Примітки

- Використовувати App Router (не Pages Router) - це нова рекомендація Next.js 14+
- TypeScript strict mode обов'язковий для якості коду
- Структура проекту повинна бути масштабованою
- Codex добре справляється з такими setup задачами

## Чому Codex?

Це типова setup задача з чіткими інструкціями. Codex:
- Знайомий з Next.js boilerplate
- Може швидко створити стандартну конфігурацію
- Не потрібні архітектурні рішення
- Шаблонна задача з чіткими кроками

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: 2025-11-16 (Codex)
