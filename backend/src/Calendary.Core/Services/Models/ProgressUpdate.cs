namespace Calendary.Core.Services.Models;

/// <summary>
/// Модель оновлення прогресу генерації зображення
/// </summary>
public class ProgressUpdate
{
    /// <summary>
    /// Прогрес у відсотках (0-100)
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Статус генерації
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Орієнтовний час до завершення (в секундах)
    /// </summary>
    public int? EstimatedTime { get; set; }

    /// <summary>
    /// Повідомлення про помилку (якщо є)
    /// </summary>
    public string? Error { get; set; }
}
