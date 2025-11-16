namespace Calendary.Model;

/// <summary>
/// Пресет свят - це набір вже налаштованих свят для різних регіонів та культур
/// (державні, релігійні, регіональні)
/// </summary>
public class HolidayPreset
{
    public int Id { get; set; }

    /// <summary>
    /// Код пресету (наприклад "UA_STATE", "US_STATE", "ORTHODOX")
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Тип пресету: "State" (державні), "Religious" (релігійні), "International" (міжнародні)
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Переклади назв та описів пресету
    /// </summary>
    public ICollection<HolidayPresetTranslation> Translations { get; set; } = new List<HolidayPresetTranslation>();

    /// <summary>
    /// Свята, що входять до цього пресету
    /// </summary>
    public ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
}
