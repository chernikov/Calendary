using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/[controller]")]
public class VerificationController : BaseUserController
{
    private readonly IEmailService _emailService;

    public VerificationController(IUserService userService, IEmailService emailService) : base(userService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-email-verification")]
    public async Task<IActionResult> SendVerificationEmail()
    {
        var user = await CurrentUser.Value;

        if (user is null)
        {
            return Unauthorized();
        }

        if (user.Email is not null && !user.IsEmailConfirmed)
        {
            // Перевіряємо, чи вже існує код, створений протягом останньої години
            var existingCode = await _userService.GetLatestVerificationEmailCodeAsync(user.Id);

            if (existingCode is not null && existingCode.ExpiryDate > DateTime.UtcNow)
            {
                return Ok(new { message = "Verification email already sent recently. Please wait before requesting a new code." });
            }


            // Генерація та збереження коду
            var verificationCode = new Random().Next(100000, 999999).ToString();
            var expiryDate = DateTime.UtcNow.AddMinutes(10);

            await _userService.CreateVerificationEmailCodeAsync(user.Id, verificationCode, expiryDate);
            await _emailService.SendVerificationEmailAsync(user.Email, verificationCode);
            return Ok(new { message = "Verification email sent." });
        }
        return BadRequest(new { message = "Verification email no need to send" });
    }

    [HttpPost("verify-email-code")]
    public async Task<IActionResult> VerifyEmailCode([FromBody] VerificationDto verification)
    {
        // Отримуємо поточного користувача
        var user = await CurrentUser.Value;

        if (user is null)
        {
            return Unauthorized();
        }

        // Перевіряємо наявність коду в базі даних для цього користувача
        var code = await _userService.GetVerificationEmailCodeAsync(user.Id, verification.VerificationCode);

        if (code is null || code.ExpiryDate < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Код перевірки не знайдено або він недійсний." });
        }

        // Підтверджуємо email користувача
        await _userService.ConfirmUserEmailAsync(user.Id);

        return Ok(new { message = "Email успішно підтверджено." });
    }
}
