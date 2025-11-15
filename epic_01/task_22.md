# Task 22: Пресети Новий рік, Різдво, Великдень

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Gemini

## Опис задачі

Створити готові пресети для популярних свят.

## Що треба зробити

1. **Новорічний пресет**:
   - 12 промптів з новорічною тематикою
   - Сніг, ялинки, подарунки, феєрверки
   - Color scheme: червоний, зелений, золотий

2. **Різдвяний пресет**:
   - Релігійні мотиви (церкви, вертепи)
   - Свічки, зірки, янголи
   - Color scheme: білий, синій, золотий

3. **Великодній пресет**:
   - Писанки, кошики, пасочки
   - Весняні квіти, зелень
   - Color scheme: пастельні кольори

4. **Літній пресет**:
   - Море, пляж, сонце, подорожі
   - Color scheme: блакитний, жовтий

5. **Осінній пресет**:
   - Жовте листя, гарбузи, урожай
   - Color scheme: помаранчевий, коричневий

## Файли для створення

- `src/Calendary.Repos/Migrations/XXXXX_SeedPresets.cs`
- `presets/new-year.json`
- `presets/christmas.json`
- `presets/easter.json`

## Приклад preset JSON

```json
{
  "name": "Новорічний",
  "nameEn": "New Year",
  "theme": "new-year",
  "colorScheme": {
    "primary": "#D42426",
    "secondary": "#2E7D32",
    "accent": "#FFD700"
  },
  "monthlyPrompts": {
    "1": "Snowy winter landscape with Christmas tree and gifts, festive atmosphere",
    "2": "Winter forest covered in snow, cozy cabin with warm lights",
    ...
  }
}
```

## Що тестувати

- [ ] 5 пресетів створені
- [ ] Промпти генерують якісні зображення
- [ ] Color schemes виглядають добре
- [ ] Локалізація UA/EN працює

---

**Створено**: 2025-11-15
