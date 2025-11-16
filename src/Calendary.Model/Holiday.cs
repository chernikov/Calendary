using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Holiday
{
    public int Id { get; set; }

    /// <summary>
    /// Дата свята (для фіксованих свят)
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Місяць свята (1-12)
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// День місяця (1-31)
    /// </summary>
    public int? Day { get; set; }

    /// <summary>
    /// Назва свята (deprecated, використовуйте Translations)
    /// </summary>
    [Obsolete("Use Translations instead")]
    public string? Name { get; set; }

    /// <summary>
    /// Чи є свято рухомим (змінюється кожен рік)
    /// </summary>
    public bool IsMovable { get; set; }

    /// <summary>
    /// Тип розрахунку для рухомих свят
    /// ("Easter_Orthodox", "Easter_Catholic", "NthWeekday" тощо)
    /// </summary>
    public string? CalculationType { get; set; }

    /// <summary>
    /// Чи є це робочий день
    /// </summary>
    public bool IsWorkingDay { get; set; }

    /// <summary>
    /// Тип свята: "State" (державне), "Religious" (релігійне), "Custom" (особисте)
    /// </summary>
    public string? Type { get; set; }

    public int? CountryId { get; set; }
    public Country? Country { get; set; }

    public int? HolidayPresetId { get; set; }
    public HolidayPreset? HolidayPreset { get; set; }

    /// <summary>
    /// Переклади назви свята різними мовами
    /// </summary>
    public ICollection<HolidayTranslation> Translations { get; set; } = new List<HolidayTranslation>();
}
