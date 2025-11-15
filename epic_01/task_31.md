# Task 31: Оновлення пакетів та Angular

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 4-6 годин
**Відповідальний AI**: Claude

## Опис задачі

Оновити всі npm пакети та мігрувати Angular до останньої стабільної версії для покращення безпеки, продуктивності та доступу до нових features.

## Проблема

GitHub виявив 23 вразливості в залежностях (4 high, 15 moderate, 4 low). Необхідно оновити пакети для закриття security issues та отримання останніх виправлень.

## Що треба зробити

### 1. Аудит поточного стану

```bash
cd src/Calendary.Ng
npm audit
npm outdated
```

Документувати:
- Список застарілих пакетів
- Критичні вразливості
- Major vs minor updates

### 2. Оновлення Angular

Перевірити поточну версію та план міграції:

```bash
ng version
ng update @angular/core@latest @angular/cli@latest --allow-dirty
```

Якщо є breaking changes:
- Переглянути Angular Update Guide: https://update.angular.io/
- Підготувати план міграції
- Оновити код згідно з новими API

### 3. Оновлення npm пакетів

**Безпечні оновлення (patch/minor):**
```bash
npm update
```

**Major оновлення (вручну):**
```bash
npm install package@latest
```

Пріоритетні пакети для оновлення:
- `@angular/*` - всі Angular пакети
- `rxjs` - для роботи з async операціями
- `typescript` - компілятор (перевірити сумісність з Angular)
- Security-критичні пакети з `npm audit`

### 4. Виправлення breaking changes

Після кожного major update:
- Запустити `npm run build`
- Виправити TypeScript помилки
- Оновити застарілі API згідно з migration guides
- Перевірити deprecated warnings

### 5. Тестування

```bash
# Build
npm run build

# Lint
npm run lint

# Run dev server
npm start

# Перевірити в браузері
# - Головна сторінка завантажується
# - Немає console errors
# - Всі роути працюють
# - API calls працюють
```

### 6. Оновлення Docker образу

Після успішного оновлення:
- Rebuild Angular Docker image
- Перевірити `docker-compose.local.yml`
- Протестувати в Docker середовищі

## Файли для зміни

- `src/Calendary.Ng/package.json` - оновлені версії пакетів
- `src/Calendary.Ng/package-lock.json` - lock file
- `src/Calendary.Ng/angular.json` - можливі зміни конфігурації
- `src/Calendary.Ng/tsconfig.json` - можливі зміни TypeScript config
- `src/Calendary.Ng/**/*.ts` - код з breaking changes
- `Dockerfile` (для Angular) - можливі зміни Node.js версії

## Що тестувати

- [ ] `npm audit` показує 0 high/critical вразливостей
- [ ] `npm run build` завершується успішно
- [ ] `npm start` запускає dev server без помилок
- [ ] Головна сторінка завантажується в браузері
- [ ] Немає console errors в браузері
- [ ] Всі існуючі features працюють коректно
- [ ] API інтеграція працює
- [ ] Docker build успішний
- [ ] Docker Compose запускає всі сервіси

## Критерії успіху

- ✅ Angular оновлено до останньої стабільної версії
- ✅ Всі npm пакети оновлені
- ✅ 0 high/critical security вразливостей
- ✅ Build проходить без помилок
- ✅ Всі існуючі функції працюють
- ✅ Docker образ перебілджено та працює

## Залежності

- Залежить від: Task 01 (базова перевірка запуску)
- Блокує: Розробку нових features з використанням нових Angular API

## Блокери

- Breaking changes в major versions Angular
- Несумісність пакетів між собою
- Зміни в TypeScript, що вимагають рефакторингу коду

## Примітки

**Безпека:**
GitHub Dependabot виявив:
- 4 high severity issues
- 15 moderate severity issues  
- 4 low severity issues

**Підхід:**
1. Спочатку patch/minor updates (безпечні)
2. Потім Angular framework update
3. Нарешті major updates інших пакетів
4. Тестування після кожного кроку

**Резервне копіювання:**
Створити окрему гілку перед початком оновлень для можливості rollback.

**Документація:**
- Angular Update Guide: https://update.angular.io/
- npm audit docs: https://docs.npmjs.com/cli/v8/commands/npm-audit
- GitHub Security Advisories: https://github.com/chernikov/Calendary/security/dependabot

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-15
