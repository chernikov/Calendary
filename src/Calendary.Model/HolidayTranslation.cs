namespace Calendary.Model;

/// <summary>
/// Переклади назв свят для різних мов
/// </summary>
public class HolidayTranslation
{
    public int Id { get; set; }

    public int HolidayId { get; set; }
    public Holiday Holiday { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    /// <summary>
    /// Назва свята відповідною мовою
    /// </summary>
    public string Name { get; set; } = null!;
}
