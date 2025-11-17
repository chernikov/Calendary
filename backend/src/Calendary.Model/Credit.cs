using System.ComponentModel.DataAnnotations;

namespace Calendary.Model;

/// <summary>
/// Кредити користувача - внутрішня валюта для оплати AI-генерації
/// </summary>
public class Credit
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// Кількість кредитів
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Тип кредитів: 'purchased' (куплені), 'bonus' (бонусні), 'referral' (реферальні), 'admin' (додані адміном)
    /// </summary>
    [MaxLength(20)]
    public string Type { get; set; } = "purchased";

    /// <summary>
    /// Дата закінчення терміну дії (null для куплених кредитів)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
