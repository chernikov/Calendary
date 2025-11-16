# Calendary.Ng Frontend

Angular 20.3+ SPA + SSR, що покриває Customer Portal Calendary. На цьому етапі виконано **Етап 1** (Task 01-03) з епіка Customer Portal:

1. **Task 01 – Frontend baseline**: Оновлено структуру проєкту, додано lint/type-check/format скрипти, підготовлено `.eslintrc` та `.prettierrc`.
2. **Task 02 – UI Kit**: Створено дизайн-токени, секції, CTA-кнопки та грід переваг для повторного використання.
3. **Task 03 – Routing & Layout**: Головний лейаут тепер містить шапку, контент і новий футер з єдиним стилем.

## Скрипти

| Скрипт | Опис |
| --- | --- |
| `npm run dev` / `npm start` | Dev-server на `http://localhost:4200` з proxy до backend. |
| `npm run build` | Production збірка + SSR bundle (`dist/calendary.ng`). |
| `npm run test` | Karma unit-тести. |
| `npm run test:e2e` | Playwright сценарії з `src/Calendary.Ng/e2e`. |
| `npm run lint` | `ng lint` із @angular-eslint (виконується після `npm install`). |
| `npm run type-check` | `tsc --noEmit` для статичного аналізу. |
| `npm run format` / `npm run format:check` | Prettier форматування та перевірка стилю. |

> ℹ️ Перший запуск `npm run lint`/`format` вимагає `npm install`, щоб підтягнути devDependencies.

## Дизайн система

- **Токени**: `src/styles/_tokens.scss` визначає кольори, шрифти, тіні, радіуси, spacing.
- **Базові стилі**: `src/styles.scss` вмикає глобальні класи (`.app-shell`, `.cta-button`, `.surface-card`, `.tag`).
- **UI компоненти** (`src/app/components/ui/`):
  - `section/section.component` – семантичні секції з різними фонами.
  - `cta-button/cta-button.component` – кнопка з варіантами `primary`/`secondary`.
  - `feature-grid/feature-grid.component` – грід карток переваг.

## Лейаут та роутинг

- `MainComponent` відповідає за спільний шаблон (header → content → footer) та показує loading overlay при блокуванні UI.
- Новий `FooterComponent` містить швидкі посилання, контакти та рік.
- `app.routes.ts` залишився джерелом істини для публічних та admin маршрутів; всі публічні сторінки вбудовані у `MainComponent`.

## Домашня сторінка

`HomeComponent` тепер використовує UI kit:
- Hero-блок з CTA, статистикою й прев’ю календаря.
- Грід переваг, які підключені через `FeatureGridComponent`.
- Таймлайн кроків створення календаря та фінальний call-to-action.

## Подальші кроки

- Розширити UI kit компонентами для інших сторінок (каталог, кошик).
- Підключити lint/format у CI pipeline.
- Додати сторінки Stage 2 (каталог шаблонів та редактор) на основі створеного фундаменту.
