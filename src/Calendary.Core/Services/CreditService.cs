using Calendary.Model;
using Calendary.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Calendary.Core.Services;

public class CreditService : ICreditService
{
    private readonly CalendaryDbContext _context;
    private readonly ILogger<CreditService> _logger;
    private readonly IConfiguration _configuration;

    // Константи для вартості AI операцій в кредитах (з документації)
    private const int COST_FINE_TUNING = 145;
    private const int COST_IMAGE_FLUX = 14;
    private const int COST_IMAGE_NANOBANANA = 3;
    private const int WELCOME_BONUS = 50;

    public CreditService(
        CalendaryDbContext context,
        ILogger<CreditService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<CreditBalanceDto> GetUserBalanceAsync(int userId)
    {
        var now = DateTime.UtcNow;

        // Підрахунок куплених кредитів
        var purchasedCredits = await _context.Credits
            .Where(c => c.UserId == userId && c.Type == "purchased")
            .SumAsync(c => c.Amount);

        // Підрахунок бонусних кредитів (що не закінчились)
        var bonusCredits = await _context.Credits
            .Where(c => c.UserId == userId
                && c.Type != "purchased"
                && (c.ExpiresAt == null || c.ExpiresAt > now))
            .SumAsync(c => c.Amount);

        // Підрахунок витрачених кредитів
        var usedCredits = await _context.CreditTransactions
            .Where(t => t.UserId == userId && t.Amount < 0)
            .SumAsync(t => Math.Abs(t.Amount));

        // Кредити що закінчуються через 30 днів
        var expiringCredits = await _context.Credits
            .Where(c => c.UserId == userId
                && c.Type != "purchased"
                && c.ExpiresAt != null
                && c.ExpiresAt <= now.AddDays(30)
                && c.ExpiresAt > now)
            .SumAsync(c => c.Amount);

        var totalAvailable = purchasedCredits + bonusCredits - usedCredits;

        return new CreditBalanceDto
        {
            Total = totalAvailable,
            Purchased = purchasedCredits - Math.Min(purchasedCredits, usedCredits - bonusCredits),
            Bonus = Math.Max(0, bonusCredits),
            ExpiringIn30Days = expiringCredits
        };
    }

    public async Task<bool> HasEnoughCreditsAsync(int userId, int amount)
    {
        var balance = await GetUserBalanceAsync(userId);
        return balance.Total >= amount;
    }

    public async Task DeductCreditsAsync(
        int userId,
        int amount,
        string type,
        string description,
        int? orderId = null,
        int? fluxModelId = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Перевірка балансу
            var balance = await GetUserBalanceAsync(userId);
            if (balance.Total < amount)
            {
                throw new InvalidOperationException($"Insufficient credits. Required: {amount}, Available: {balance.Total}");
            }

            var now = DateTime.UtcNow;
            int remaining = amount;

            // Спочатку списуємо бонусні кредити (ті, що скоро закінчуються)
            var bonusCredits = await _context.Credits
                .Where(c => c.UserId == userId
                    && c.Type != "purchased"
                    && (c.ExpiresAt == null || c.ExpiresAt > now))
                .OrderBy(c => c.ExpiresAt ?? DateTime.MaxValue)
                .ToListAsync();

            foreach (var credit in bonusCredits)
            {
                if (remaining <= 0) break;

                int toDeduct = Math.Min(credit.Amount, remaining);
                credit.Amount -= toDeduct;
                remaining -= toDeduct;

                if (credit.Amount == 0)
                {
                    _context.Credits.Remove(credit);
                }
            }

            // Якщо бонусів не вистачило, списуємо куплені
            if (remaining > 0)
            {
                var purchasedCredits = await _context.Credits
                    .Where(c => c.UserId == userId && c.Type == "purchased")
                    .FirstOrDefaultAsync();

                if (purchasedCredits == null || purchasedCredits.Amount < remaining)
                {
                    throw new InvalidOperationException("Insufficient purchased credits");
                }

                purchasedCredits.Amount -= remaining;
                if (purchasedCredits.Amount == 0)
                {
                    _context.Credits.Remove(purchasedCredits);
                }
            }

            // Записуємо транзакцію
            var creditTransaction = new CreditTransaction
            {
                UserId = userId,
                Amount = -amount,
                Type = type,
                Description = description,
                OrderId = orderId,
                FluxModelId = fluxModelId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.CreditTransactions.AddAsync(creditTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Deducted {Amount} credits from user {UserId}. Type: {Type}",
                amount, userId, type);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deducting credits for user {UserId}", userId);
            throw;
        }
    }

    public async Task AddCreditsAsync(
        int userId,
        int amount,
        string type,
        string description,
        DateTime? expiresAt = null,
        int? creditPackageId = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Додаємо кредити
            var credit = new Credit
            {
                UserId = userId,
                Amount = amount,
                Type = type,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Credits.AddAsync(credit);

            // Записуємо транзакцію
            var creditTransaction = new CreditTransaction
            {
                UserId = userId,
                Amount = amount,
                Type = type,
                Description = description,
                CreditPackageId = creditPackageId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.CreditTransactions.AddAsync(creditTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Added {Amount} credits to user {UserId}. Type: {Type}",
                amount, userId, type);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adding credits for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<CreditPackage>> GetActiveCreditPackagesAsync()
    {
        return await _context.CreditPackages
            .Where(cp => cp.IsActive)
            .OrderBy(cp => cp.DisplayOrder)
            .ToListAsync();
    }

    public async Task<CreditPackage?> GetCreditPackageByIdAsync(int packageId)
    {
        return await _context.CreditPackages
            .FirstOrDefaultAsync(cp => cp.Id == packageId && cp.IsActive);
    }

    public async Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int skip = 0, int take = 50)
    {
        return await _context.CreditTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Include(t => t.Order)
            .Include(t => t.FluxModel)
            .Include(t => t.CreditPackage)
            .ToListAsync();
    }

    public async Task ProcessCreditPackagePurchaseAsync(int userId, int packageId, string invoiceId)
    {
        var package = await GetCreditPackageByIdAsync(packageId);
        if (package == null)
        {
            throw new InvalidOperationException($"Credit package {packageId} not found or inactive");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Додаємо куплені кредити
            await AddCreditsAsync(
                userId,
                package.Credits,
                "purchased",
                $"Purchased {package.Name} package (Invoice: {invoiceId})",
                expiresAt: null,
                creditPackageId: package.Id
            );

            // Додаємо бонусні кредити якщо є
            if (package.BonusCredits > 0)
            {
                await AddCreditsAsync(
                    userId,
                    package.BonusCredits,
                    "bonus",
                    $"Bonus credits from {package.Name} package",
                    expiresAt: DateTime.UtcNow.AddYears(1),
                    creditPackageId: package.Id
                );
            }

            await transaction.CommitAsync();

            _logger.LogInformation(
                "Processed credit package purchase for user {UserId}. Package: {PackageName}, Credits: {Credits}, Bonus: {Bonus}",
                userId, package.Name, package.Credits, package.BonusCredits);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing credit package purchase for user {UserId}", userId);
            throw;
        }
    }

    public async Task AddWelcomeBonusAsync(int userId)
    {
        // Перевірка чи користувач вже отримував вітальний бонус
        var hasWelcomeBonus = await _context.CreditTransactions
            .AnyAsync(t => t.UserId == userId && t.Type == "welcome_bonus");

        if (hasWelcomeBonus)
        {
            _logger.LogWarning("User {UserId} already received welcome bonus", userId);
            return;
        }

        await AddCreditsAsync(
            userId,
            WELCOME_BONUS,
            "welcome_bonus",
            "Welcome bonus for new user",
            expiresAt: DateTime.UtcNow.AddYears(1)
        );

        _logger.LogInformation("Added welcome bonus to user {UserId}", userId);
    }

    public async Task AddCreditsByAdminAsync(int userId, int amount, string reason)
    {
        await AddCreditsAsync(
            userId,
            amount,
            "admin",
            $"Added by admin: {reason}",
            expiresAt: null
        );

        _logger.LogInformation(
            "Admin added {Amount} credits to user {UserId}. Reason: {Reason}",
            amount, userId, reason);
    }
}
