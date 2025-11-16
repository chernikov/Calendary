using System.ComponentModel.DataAnnotations;

namespace Calendary.Model;

/// <summary>
/// Історія транзакцій кредитів
/// </summary>
public class CreditTransaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// Кількість кредитів (позитивне = зарахування, негативне = списання)
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Тип транзакції: 'fine_tuning', 'image_generation', 'purchase', 'bonus', 'admin_add', 'refund'
    /// </summary>
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Опис транзакції
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// ID замовлення (якщо транзакція пов'язана з замовленням)
    /// </summary>
    public int? OrderId { get; set; }

    public Order? Order { get; set; }

    /// <summary>
    /// ID FluxModel (якщо транзакція пов'язана з генерацією моделі)
    /// </summary>
    public int? FluxModelId { get; set; }

    public FluxModel? FluxModel { get; set; }

    /// <summary>
    /// ID пакету кредитів (якщо це покупка)
    /// </summary>
    public int? CreditPackageId { get; set; }

    public CreditPackage? CreditPackage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
