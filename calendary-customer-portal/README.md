# Calendary Customer Portal

Frontend проект на Next.js 14 для публічної частини Calendary. Містить базові сторінки, UI Kit на Tailwind + shadcn/ui та готовий layout для подальшої розробки.

## Швидкий старт

```bash
cd calendary-customer-portal
npm install
npm run dev
```

- `npm run build` — production збірка
- `npm run lint` — перевірка ESLint
- `npm run type-check` — строгий TypeScript

## Структура

```
src/
  app/          # App Router сторінки
  components/
    layout/     # Header, Footer, Container
    ui/         # shadcn/ui компоненти
  lib/          # хелпери та константи
  styles/       # Tailwind стилі
```

## Залежності

- Next.js 14
- React 18
- Tailwind CSS + tailwindcss-animate
- shadcn/ui (Radix UI + class-variance-authority)
- Zustand, React Hook Form, Zod
- next-themes для темної теми

## Середовище

Скопіюйте `.env.local.example` у `.env.local` і оновіть API URL при необхідності.
