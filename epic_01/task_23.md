# Task 23: UI для вибору пресета

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Низька
**Час**: 2-3 години
**Відповідальний AI**: Frontend Dev / Claude Code

## Опис задачі

Створити UI для перегляду та вибору пресетів в /editor.

## Що треба зробити

1. **Presets Gallery**:
   - Grid з карточками пресетів
   - Preview зображення
   - Назва та опис
   - "Застосувати" button

2. **Preset Card**:
   ```html
   <div class="preset-card">
     <img src="preview.jpg" />
     <h3>Новорічний</h3>
     <p>Святкова атмосфера Нового року</p>
     <button>Застосувати</button>
   </div>
   ```

3. **Apply Modal**:
   - Попередження що поточні зображення будуть замінені
   - Preview першого зображення з пресета
   - Підтвердження

## Файли для створення

- `src/Calendary.Ng/src/app/pages/editor/components/presets-gallery/presets-gallery.component.ts`
- `src/Calendary.Ng/src/app/pages/editor/services/preset.service.ts`

## Що тестувати

- [ ] Gallery завантажується
- [ ] Пресети відображаються
- [ ] Click "Застосувати" працює
- [ ] Підтвердження показується
- [ ] Preview коректний

---

**Створено**: 2025-11-15
