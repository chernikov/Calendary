# Task 12: Перемикач типу промпту (свій/покращений)

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE ✅
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: GPT/Codex

## Опис задачі

Додати toggle в UI генерації: використовувати промпт користувача "як є" або покращити через AI.

## Що треба зробити

1. **UI Toggle**:
   ```html
   <mat-slide-toggle [(ngModel)]="useEnhancedPrompt">
     Покращити промпт через AI
   </mat-slide-toggle>
   ```

2. **Backend Logic**:
   - Якщо `useEnhancedPrompt = true` → викликати Claude/GPT для enhance
   - Інакше → використовувати промпт як є

3. **Show Enhanced Prompt**:
   - Показати користувачу покращений промпт перед генерацією
   - Дати можливість edit або accept

## Файли для створення

- `src/Calendary.Core/Services/PromptEnhancerService.cs`

## Файли для зміни

- `src/Calendary.Ng/src/app/pages/editor/components/generate-modal/generate-modal.component.html`
- `src/Calendary.Api/Controllers/SynthesisController.cs`

## Що тестувати

- [x] Toggle працює
- [x] Enhanced prompt генерується
- [x] Показується користувачу перед accept
- [x] Можна edit enhanced prompt
- [x] Генерація використовує правильний промпт

---

**Створено**: 2025-11-15
