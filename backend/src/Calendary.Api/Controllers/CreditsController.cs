using Calendary.Api.Tools;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Route("api/credits")]
[Authorize]
public class CreditsController : BaseUserController
{
    private readonly ICreditService _creditService;
    private readonly IPaymentService _paymentService;

    public CreditsController(
        IUserService userService,
        ICreditService creditService,
        IPaymentService paymentService) : base(userService)
    {
        _creditService = creditService;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Отримати баланс кредитів поточного користувача
    /// GET /api/credits/balance
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var balance = await _creditService.GetUserBalanceAsync(user.Id);
        return Ok(balance);
    }

    /// <summary>
    /// Отримати список активних пакетів кредитів
    /// GET /api/credits/packages
    /// </summary>
    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages()
    {
        var packages = await _creditService.GetActiveCreditPackagesAsync();
        return Ok(packages);
    }

    /// <summary>
    /// Купити пакет кредитів
    /// POST /api/credits/purchase
    /// </summary>
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchasePackage([FromBody] PurchaseCreditPackageRequest request)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        // Перевірка що пакет існує
        var package = await _creditService.GetCreditPackageByIdAsync(request.PackageId);
        if (package == null)
        {
            return NotFound(new { message = "Credit package not found" });
        }

        // Створення invoice в MonoBank
        var paymentUrl = await _paymentService.CreateCreditPackageInvoiceAsync(
            user.Id,
            package.Id,
            package.PriceUAH,
            package.Name
        );

        return Ok(new
        {
            paymentUrl,
            package = new
            {
                package.Id,
                package.Name,
                package.Credits,
                package.BonusCredits,
                package.PriceUAH
            }
        });
    }

    /// <summary>
    /// Отримати історію транзакцій кредитів
    /// GET /api/credits/transactions?skip=0&take=50
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        if (take > 100)
        {
            take = 100; // Максимум 100 за раз
        }

        var transactions = await _creditService.GetUserTransactionsAsync(user.Id, skip, take);

        var result = transactions.Select(t => new
        {
            t.Id,
            t.Amount,
            t.Type,
            t.Description,
            t.CreatedAt,
            Order = t.Order != null ? new { t.Order.Id, t.Order.OrderDate } : null,
            FluxModel = t.FluxModel != null ? new { t.FluxModel.Id, t.FluxModel.Name } : null,
            CreditPackage = t.CreditPackage != null ? new { t.CreditPackage.Id, t.CreditPackage.Name } : null
        });

        return Ok(result);
    }

    /// <summary>
    /// Перевірити чи достатньо кредитів для операції
    /// GET /api/credits/check?amount=145
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> CheckBalance([FromQuery] int amount)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var balance = await _creditService.GetUserBalanceAsync(user.Id);
        var hasEnough = balance.Total >= amount;

        return Ok(new
        {
            required = amount,
            available = balance.Total,
            hasEnough,
            shortfall = hasEnough ? 0 : amount - balance.Total
        });
    }
}

public class PurchaseCreditPackageRequest
{
    public int PackageId { get; set; }
}
