using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calendary.Model;

/// <summary>
/// Пакет кредитів для продажу
/// </summary>
public class CreditPackage
{
    public int Id { get; set; }

    /// <summary>
    /// Назва пакету (Starter, Basic, Standard, Premium, Business)
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Кількість базових кредитів
    /// </summary>
    public int Credits { get; set; }

    /// <summary>
    /// Бонусні кредити
    /// </summary>
    public int BonusCredits { get; set; }

    /// <summary>
    /// Ціна в гривнях
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceUAH { get; set; }

    /// <summary>
    /// Чи активний пакет
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Опис пакету
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Порядок відображення (для сортування)
    /// </summary>
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
