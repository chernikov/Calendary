using Calendary.Model;

namespace Calendary.Core.Services;

/// <summary>
/// Сервіс для управління кредитами користувачів
/// </summary>
public interface ICreditService
{
    /// <summary>
    /// Отримати баланс кредитів користувача
    /// </summary>
    Task<CreditBalanceDto> GetUserBalanceAsync(int userId);

    /// <summary>
    /// Перевірити чи достатньо кредитів
    /// </summary>
    Task<bool> HasEnoughCreditsAsync(int userId, int amount);

    /// <summary>
    /// Списати кредити
    /// </summary>
    Task DeductCreditsAsync(int userId, int amount, string type, string description, int? orderId = null, int? fluxModelId = null);

    /// <summary>
    /// Додати кредити
    /// </summary>
    Task AddCreditsAsync(int userId, int amount, string type, string description, DateTime? expiresAt = null, int? creditPackageId = null);

    /// <summary>
    /// Отримати активні пакети кредитів
    /// </summary>
    Task<List<CreditPackage>> GetActiveCreditPackagesAsync();

    /// <summary>
    /// Отримати пакет кредитів за ID
    /// </summary>
    Task<CreditPackage?> GetCreditPackageByIdAsync(int packageId);

    /// <summary>
    /// Отримати історію транзакцій користувача
    /// </summary>
    Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int skip = 0, int take = 50);

    /// <summary>
    /// Обробити успішну оплату пакету кредитів
    /// </summary>
    Task ProcessCreditPackagePurchaseAsync(int userId, int packageId, string invoiceId);

    /// <summary>
    /// Додати вітальний бонус новому користувачу
    /// </summary>
    Task AddWelcomeBonusAsync(int userId);

    /// <summary>
    /// Додати кредити вручну (адміністратор)
    /// </summary>
    Task AddCreditsByAdminAsync(int userId, int amount, string reason);
}

public class CreditBalanceDto
{
    public int Total { get; set; }
    public int Purchased { get; set; }
    public int Bonus { get; set; }
    public int ExpiringIn30Days { get; set; }
}
