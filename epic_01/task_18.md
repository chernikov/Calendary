# Task 18: Генерація сітки днів на 2026 рік

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P0 (Критичний)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Gemini

## Опис задачі

Реалізувати логіку генерації сітки днів для кожного місяця 2026 року.

## Що треба зробити

1. **MonthGridGenerator**:
   ```csharp
   public class MonthGridGenerator
   {
       public MonthGrid Generate(int year, int month, DayOfWeek startDay)
       {
           var firstDay = new DateTime(year, month, 1);
           var daysInMonth = DateTime.DaysInMonth(year, month);

           // Generate grid with weeks
           // Fill empty cells before first day
           // Fill days of month
           // Fill empty cells after last day

           return new MonthGrid { Weeks = weeks };
       }
   }
   ```

2. **Week structure**:
   - 7 cells per week
   - Empty cells для вирівнювання
   - Weekend highlighting
   - Current day highlighting (optional)

3. **Flexible start day**:
   - Monday (default for Ukraine)
   - Sunday (US style)
   - Configurable per user

## Файли для створення

- `src/Calendary.Core/Services/MonthGridGenerator.cs`
- `src/Calendary.Core/Models/MonthGrid.cs`
- `src/Calendary.Core/Models/DayCell.cs`

## Що тестувати

- [ ] Січень 2026 має правильну кількість днів (31)
- [ ] Перший день січня 2026 - четвер
- [ ] Лютий 2026 має 28 днів (не високосний)
- [ ] Всі місяці генеруються коректно
- [ ] Start day змінюється (Monday/Sunday)
- [ ] Empty cells заповнюються

---

**Створено**: 2025-11-15
