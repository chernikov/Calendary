namespace Calendary.Core.Services;

/// <summary>
/// Калькулятор для розрахунку дати Великодня та інших рухомих свят
/// </summary>
public class EasterCalculator
{
    /// <summary>
    /// Розрахунок православного Великодня (за юліанським календарем)
    /// Використовує алгоритм Гауса для юліанського календаря
    /// </summary>
    /// <param name="year">Рік</param>
    /// <returns>Дата православного Великодня</returns>
    public static DateTime CalculateOrthodoxEaster(int year)
    {
        // Алгоритм Гауса для юліанського календаря
        int a = year % 19;
        int b = year % 4;
        int c = year % 7;
        int d = (19 * a + 15) % 30;
        int e = (2 * b + 4 * c + 6 * d + 6) % 7;
        int f = d + e;

        // Визначення дати Великодня
        int day, month;
        if (f <= 9)
        {
            day = f + 22; // Березень
            month = 3;
        }
        else
        {
            day = f - 9; // Квітень
            month = 4;
        }

        // Конвертація з юліанського в григоріанський календар
        var julianDate = new DateTime(year, month, day);

        // Різниця між григоріанським та юліанським календарем для XX-XXI століття
        int gregorianCorrection = 13;
        return julianDate.AddDays(gregorianCorrection);
    }

    /// <summary>
    /// Розрахунок католицького Великодня (за григоріанським календарем)
    /// Використовує алгоритм Computus (метод Гауса)
    /// </summary>
    /// <param name="year">Рік</param>
    /// <returns>Дата католицького Великодня</returns>
    public static DateTime CalculateCatholicEaster(int year)
    {
        // Алгоритм Computus (метод Гауса)
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

    /// <summary>
    /// Розрахунок Вознесіння Господнього (40-й день після Великодня)
    /// </summary>
    public static DateTime CalculateAscension(DateTime easter)
    {
        return easter.AddDays(39); // 40-й день включає сам Великдень
    }

    /// <summary>
    /// Розрахунок Трійці/П'ятдесятниці (50-й день після Великодня)
    /// </summary>
    public static DateTime CalculatePentecost(DateTime easter)
    {
        return easter.AddDays(49); // 50-й день включає сам Великдень
    }

    /// <summary>
    /// Розрахунок Попільної середи (46 днів до католицького Великодня)
    /// </summary>
    public static DateTime CalculateAshWednesday(DateTime catholicEaster)
    {
        return catholicEaster.AddDays(-46);
    }

    /// <summary>
    /// Розрахунок N-го дня тижня в місяці
    /// Наприклад: 3-й понеділок січня, останній понеділок травня
    /// </summary>
    /// <param name="year">Рік</param>
    /// <param name="month">Місяць (1-12)</param>
    /// <param name="dayOfWeek">День тижня</param>
    /// <param name="occurrence">Порядковий номер (1-5, або -1 для останнього)</param>
    /// <returns>Дата</returns>
    public static DateTime CalculateNthWeekday(int year, int month, DayOfWeek dayOfWeek, int occurrence)
    {
        if (occurrence == -1)
        {
            // Останній такий день тижня в місяці
            var lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            while (lastDayOfMonth.DayOfWeek != dayOfWeek)
            {
                lastDayOfMonth = lastDayOfMonth.AddDays(-1);
            }

            return lastDayOfMonth;
        }
        else
        {
            // N-й такий день тижня в місяці
            var firstDayOfMonth = new DateTime(year, month, 1);
            int daysUntilTarget = ((int)dayOfWeek - (int)firstDayOfMonth.DayOfWeek + 7) % 7;
            var firstOccurrence = firstDayOfMonth.AddDays(daysUntilTarget);

            return firstOccurrence.AddDays(7 * (occurrence - 1));
        }
    }
}
