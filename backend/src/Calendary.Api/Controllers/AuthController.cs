using Calendary.Api.Auth;
using Calendary.Api.Dtos;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IPasswordAuthService passwordAuth,
    IGoogleAuthService googleAuth,
    ISessionTokenService sessionTokens,
    IEmailService email,
    ILogger<AuthController> logger,
    AppDbContext db) : ControllerBase
{
    private const int MinPasswordLength = 8;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return BadRequest("A valid email is required.");
        }
        if (request.Password.Length < MinPasswordLength)
        {
            return BadRequest($"Password must be at least {MinPasswordLength} characters.");
        }

        var user = await passwordAuth.RegisterAsync(request.Email, request.Password, request.DisplayName);
        if (user is null)
        {
            return Conflict("This email is already registered.");
        }

        var bearer = await sessionTokens.IssueTokenAsync(user);

        try
        {
            await email.SendAsync(
                user.Email!,
                "Ласкаво просимо до Calendary",
                $"<p>Привіт, {System.Net.WebUtility.HtmlEncode(user.DisplayName)}!</p>" +
                "<p>Дякуємо за реєстрацію в Calendary. Можете одразу починати збирати свій календар.</p>" +
                $"<p>Щоб підтвердити пошту, введіть цей код у застосунку: " +
                $"<strong style=\"font-size:20px;letter-spacing:4px;\">{user.EmailConfirmationCode}</strong></p>" +
                "<p>Код дійсний 30 хвилин.</p>");
        }
        catch (Exception ex)
        {
            // A failed welcome email should never fail registration itself.
            logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        return Ok(new AuthResponse(bearer, user.ToDto()));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await passwordAuth.LoginAsync(request.Email, request.Password);
        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var bearer = await sessionTokens.IssueTokenAsync(user);
        return Ok(new AuthResponse(bearer, user.ToDto()));
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Google(GoogleAuthRequest request)
    {
        User googleUser;
        try
        {
            googleUser = await googleAuth.AuthenticateAsync(request.IdToken);
        }
        catch (InvalidGoogleTokenException)
        {
            return Unauthorized("Invalid Google credential.");
        }

        var bearer = await sessionTokens.IssueTokenAsync(googleUser);
        return Ok(new AuthResponse(bearer, googleUser.ToDto()));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = User.GetUserId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user is null ? Unauthorized() : Ok(user.ToDto());
    }

    [HttpPost("confirm-email")]
    [Authorize]
    public async Task<ActionResult<UserDto>> ConfirmEmail(ConfirmEmailRequest request)
    {
        var userId = User.GetUserId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return Unauthorized();
        }
        if (user.EmailConfirmed)
        {
            return Ok(user.ToDto());
        }

        var code = request.Code?.Trim() ?? string.Empty;
        if (user.EmailConfirmationCode is null
            || user.EmailConfirmationCodeExpiresAtUtc is null
            || user.EmailConfirmationCodeExpiresAtUtc < DateTime.UtcNow
            || !string.Equals(user.EmailConfirmationCode, code, StringComparison.Ordinal))
        {
            return BadRequest("Невірний або прострочений код.");
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiresAtUtc = null;
        await db.SaveChangesAsync();

        return Ok(user.ToDto());
    }

    [HttpPost("resend-confirmation")]
    [Authorize]
    public async Task<IActionResult> ResendConfirmation()
    {
        var userId = User.GetUserId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return Unauthorized();
        }
        if (user.EmailConfirmed)
        {
            return Ok();
        }

        user.EmailConfirmationCode = EmailConfirmationCodeGenerator.Generate();
        user.EmailConfirmationCodeExpiresAtUtc = DateTime.UtcNow.Add(EmailConfirmationCodeGenerator.Lifetime);
        await db.SaveChangesAsync();

        try
        {
            await email.SendAsync(
                user.Email!,
                "Код підтвердження Calendary",
                "<p>Ваш код підтвердження пошти:</p>" +
                $"<p><strong style=\"font-size:20px;letter-spacing:4px;\">{user.EmailConfirmationCode}</strong></p>" +
                "<p>Код дійсний 30 хвилин.</p>");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send confirmation email to {Email}", user.Email);
        }

        return Ok();
    }
}
