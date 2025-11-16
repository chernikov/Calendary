# Task 24: Застосування пресетів свят до календаря

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: DONE
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude
**Виконано**: 16.11.2025

## Опис задачі

Реалізувати backend логіку застосування одного або кількох пресетів свят до календаря користувача. Включає розрахунок рухомих свят, обробку дублікатів та збереження свят у базі даних.

## Що треба зробити

1. **Backend API endpoint**:
   ```csharp
   [HttpPost("calendars/{calendarId}/apply-presets")]
   public async Task<IActionResult> ApplyHolidayPresets(
       int calendarId, 
       [FromBody] ApplyPresetsRequest request)
   {
       var userId = User.GetUserId();
       var calendar = await _calendarRepo.GetByIdAsync(calendarId);
       
       if (calendar.UserId != userId)
           return Forbid();

       var result = await _holidayPresetService.ApplyPresetsToCalendarAsync(
           calendarId, 
           request.PresetCodes, 
           request.Year,
           request.ReplaceExisting
       );

       return Ok(result);
   }

   public class ApplyPresetsRequest
   {
       public List<string> PresetCodes { get; set; } // ["UA_STATE", "ORTHODOX"]
       public int Year { get; set; } // 2026
       public bool ReplaceExisting { get; set; } // true/false
   }

   public class ApplyPresetsResult
   {
       public int AddedCount { get; set; }
       public int DuplicatesSkipped { get; set; }
       public int ReplacedCount { get; set; }
       public List<CalendarHoliday> Holidays { get; set; }
   }
   ```

2. **HolidayPresetService.ApplyPresetsToCalendarAsync()**:
   ```csharp
   public async Task<ApplyPresetsResult> ApplyPresetsToCalendarAsync(
       int calendarId, 
       List<string> presetCodes, 
       int year,
       bool replaceExisting)
   {
       var result = new ApplyPresetsResult();
       
       // 1. Завантажити всі пресети
       var presets = await _presetRepo.GetByCodesAsync(presetCodes);
       
       // 2. Розрахувати всі свята для вказаного року
       var allHolidays = new List<ResolvedHoliday>();
       foreach (var preset in presets)
       {
           var holidays = await ResolveHolidaysForYear(preset, year);
           allHolidays.AddRange(holidays);
       }
       
       // 3. Видалити дублікати (одне свято з кількох пресетів)
       var uniqueHolidays = RemoveDuplicates(allHolidays);
       
       // 4. Обробити існуючі свята в календарі
       if (replaceExisting)
       {
           await _calendarHolidayRepo.DeleteByCalendarIdAsync(calendarId);
           result.ReplacedCount = uniqueHolidays.Count;
       }
       else
       {
           var existing = await _calendarHolidayRepo
               .GetByCalendarIdAsync(calendarId);
           uniqueHolidays = FilterOutExisting(uniqueHolidays, existing);
           result.DuplicatesSkipped = existing.Count;
       }
       
       // 5. Додати свята до календаря
       var calendarHolidays = uniqueHolidays.Select(h => new CalendarHoliday
       {
           CalendarId = calendarId,
           Date = h.Date,
           Name = h.Name,
           HolidayId = h.HolidayId,
           IsHighlighted = true
       }).ToList();
       
       await _calendarHolidayRepo.AddRangeAsync(calendarHolidays);
       
       result.AddedCount = calendarHolidays.Count;
       result.Holidays = calendarHolidays;
       
       return result;
   }
   ```

3. **Розрахунок рухомих свят**:
   ```csharp
   private async Task<List<ResolvedHoliday>> ResolveHolidaysForYear(
       HolidayPreset preset, 
       int year)
   {
       var resolved = new List<ResolvedHoliday>();
       
       foreach (var holiday in preset.Holidays)
       {
           DateTime date;
           
           if (!holiday.IsMovable)
           {
               // Фіксоване свято
               date = new DateTime(year, holiday.Month, holiday.Day);
           }
           else
           {
               // Рухоме свято - використати калькулятор
               date = holiday.CalculationType switch
               {
                   "Easter_Orthodox" => _easterCalculator.GetOrthodoxEaster(year),
                   "Easter_Catholic" => _easterCalculator.GetCatholicEaster(year),
                   "EasterOffset" => GetEasterOffset(holiday, year),
                   "NthWeekday" => GetNthWeekdayOfMonth(holiday, year),
                   _ => throw new NotSupportedException()
               };
           }
           
           resolved.Add(new ResolvedHoliday
           {
               HolidayId = holiday.Id,
               Date = date,
               Name = holiday.Name, // вже локалізована
               PresetCode = preset.Code
           });
       }
       
       return resolved;
   }
   
   private DateTime GetEasterOffset(Holiday holiday, int year)
   {
       // Для свят як Вознесіння (40 днів після Великодня)
       var easter = holiday.CalculationType.Contains("Orthodox")
           ? _easterCalculator.GetOrthodoxEaster(year)
           : _easterCalculator.GetCatholicEaster(year);
           
       return easter.AddDays(holiday.DaysFromEaster);
   }
   
   private DateTime GetNthWeekdayOfMonth(Holiday holiday, int year)
   {
       // Для свят як "3-й понеділок січня" (Martin Luther King Jr. Day)
       var firstDay = new DateTime(year, holiday.Month, 1);
       var firstWeekday = GetFirstWeekday(firstDay, holiday.DayOfWeek);
       return firstWeekday.AddDays((holiday.WeekNumber - 1) * 7);
   }
   ```

4. **EasterCalculator Service** (Computus algorithm):
   ```csharp
   public class EasterCalculator
   {
       public DateTime GetOrthodoxEaster(int year)
       {
           // Юліанський календар, конвертований до григоріанського
           int a = year % 19;
           int b = year % 4;
           int c = year % 7;
           int d = (19 * a + 15) % 30;
           int e = (2 * b + 4 * c + 6 * d + 6) % 7;
           int day = d + e + 3;
           
           DateTime julianDate = new DateTime(year, 3, 22).AddDays(day);
           // Додати 13 днів для григоріанського календаря (для 20-21 століття)
           return julianDate.AddDays(13);
       }
       
       public DateTime GetCatholicEaster(int year)
       {
           // Григоріанський календар (Computus)
           int a = year % 19;
           int b = year / 100;
           int c = year % 100;
           int d = b / 4;
           int e = b % 4;
           int f = (b + 8) / 25;
           int g = (b - f + 1) / 3;
           int h = (19 * a + b - d - g + 15) % 30;
           int i = c / 4;
           int k = c % 4;
           int l = (32 + 2 * e + 2 * i - h - k) % 7;
           int m = (a + 11 * h + 22 * l) / 451;
           int month = (h + l - 7 * m + 114) / 31;
           int day = ((h + l - 7 * m + 114) % 31) + 1;
           
           return new DateTime(year, month, day);
       }
   }
   ```

5. **Обробка дублікатів**:
   - Порівняння за датою ТА назвою
   - Якщо одне свято в кількох пресетах - взяти тільки одне
   - Пріоритет: більш специфічний пресет (ORTHODOX > UA_STATE для Різдва 7 січня)

6. **CalendarHoliday entity**:
   ```csharp
   public class CalendarHoliday
   {
       public int Id { get; set; }
       public int CalendarId { get; set; }
       public int HolidayId { get; set; }
       public DateTime Date { get; set; } // Розрахована дата для конкретного року
       public string Name { get; set; } // Локалізована назва
       public bool IsHighlighted { get; set; } // Підсвічувати в календарі
       
       public Calendar Calendar { get; set; }
       public Holiday Holiday { get; set; }
   }
   ```

## Файли для створення/зміни

- `src/Calendary.Api/Controllers/CalendarController.cs` (новий endpoint)
- `src/Calendary.Core/Services/HolidayPresetService.cs`
- `src/Calendary.Core/Services/EasterCalculator.cs`
- `src/Calendary.Core/Services/MovableHolidayCalculator.cs`
- `src/Calendary.Model/CalendarHoliday.cs`
- `src/Calendary.Repos/Repositories/CalendarHolidayRepository.cs`
- Міграція для таблиці `CalendarHolidays`

## Що тестувати

- [ ] Застосування одного пресету додає всі свята
- [ ] Застосування кількох пресетів об'єднує свята
- [ ] Дублікати видаляються коректно
- [ ] ReplaceExisting=true видаляє старі свята
- [ ] ReplaceExisting=false зберігає існуючі
- [ ] Рухомі свята розраховуються правильно:
  - [ ] Православний Великдень 2026: 19 квітня
  - [ ] Католицький Великдень 2026: 5 квітня
  - [ ] Трійця 2026 (православна): 7 червня (50 днів після 19 квітня)
  - [ ] Вознесіння 2026 (католицьке): 14 травня (40 днів після 5 квітня)
- [ ] NthWeekday свята розраховуються (Thanksgiving, MLK Day)
- [ ] API повертає правильну кількість доданих/пропущених свят
- [ ] Локалізовані назви зберігаються в CalendarHoliday
- [ ] Транзакції: rollback при помилках

---

**Створено**: 2025-11-15
**Оновлено**: 2025-11-16
