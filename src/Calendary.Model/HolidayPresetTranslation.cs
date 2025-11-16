namespace Calendary.Model;

/// <summary>
/// Переклади назв та описів пресетів свят для різних мов
/// </summary>
public class HolidayPresetTranslation
{
    public int Id { get; set; }

    public int HolidayPresetId { get; set; }
    public HolidayPreset HolidayPreset { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    /// <summary>
    /// Назва пресету відповідною мовою
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Опис пресету відповідною мовою
    /// </summary>
    public string? Description { get; set; }
}
