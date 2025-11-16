using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Route("api/admin/credits")]
[Authorize(Roles = "Admin")]
public class AdminCreditsController : ControllerBase
{
    private readonly ICreditService _creditService;
    private readonly IUserService _userService;

    public AdminCreditsController(
        ICreditService creditService,
        IUserService userService)
    {
        _creditService = creditService;
        _userService = userService;
    }

    /// <summary>
    /// Додати кредити користувачу (тільки адмін)
    /// POST /api/admin/credits/add
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> AddCreditsToUser([FromBody] AddCreditsRequest request)
    {
        // Перевірка що користувач існує
        var user = await _userService.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Додати кредити
        await _creditService.AddCreditsByAdminAsync(
            request.UserId,
            request.Amount,
            request.Reason ?? "Added by admin"
        );

        // Отримати новий баланс
        var balance = await _creditService.GetUserBalanceAsync(request.UserId);

        return Ok(new
        {
            message = $"Successfully added {request.Amount} credits to user {request.UserId}",
            balance
        });
    }

    /// <summary>
    /// Отримати баланс кредитів будь-якого користувача
    /// GET /api/admin/credits/balance/{userId}
    /// </summary>
    [HttpGet("balance/{userId:int}")]
    public async Task<IActionResult> GetUserBalance(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var balance = await _creditService.GetUserBalanceAsync(userId);
        return Ok(new
        {
            userId,
            userName = user.UserName,
            email = user.Email,
            balance
        });
    }

    /// <summary>
    /// Отримати транзакції користувача
    /// GET /api/admin/credits/transactions/{userId}
    /// </summary>
    [HttpGet("transactions/{userId:int}")]
    public async Task<IActionResult> GetUserTransactions(int userId, [FromQuery] int skip = 0, [FromQuery] int take = 100)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var transactions = await _creditService.GetUserTransactionsAsync(userId, skip, take);

        return Ok(new
        {
            userId,
            userName = user.UserName,
            transactions = transactions.Select(t => new
            {
                t.Id,
                t.Amount,
                t.Type,
                t.Description,
                t.CreatedAt
            })
        });
    }
}

public class AddCreditsRequest
{
    public int UserId { get; set; }
    public int Amount { get; set; }
    public string? Reason { get; set; }
}
