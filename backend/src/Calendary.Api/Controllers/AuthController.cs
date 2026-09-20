using Calendary.Api.Auth;
using Calendary.Api.Dtos;
using Calendary.Api.Filters;
using Calendary.Application.Auth;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Calendary.Api.Controllers;

// Every action here is anonymous-reachable (register/login/google/forgot-password/reset-password)
// or cheap to spam (confirm-email/resend-confirmation) — the whole controller sits behind the
// "auth" rate-limit policy (Program.cs: 10 req/min per client IP) rather than picking endpoints
// one by one (#301).
[ApiController]
[Route("api/auth")]
[TypeFilter(typeof(AppOperationExceptionFilter))]
[EnableRateLimiting("auth")]
public class AuthController(
    IPasswordAuthService passwordAuth,
    IGoogleAuthService googleAuth,
    ISessionTokenService sessionTokens,
    IEmailService email,
    ILogger<AuthController> logger,
    ISender sender,
    // Reused rather than introducing a parallel "app's own public origin" option — it's already
    // wired to the same value (https://$DOMAIN / https://$STAGING_DOMAIN, empty locally) in every
    // environment for building Monobank's redirect/webhook URLs, and a password-reset link needs
    // exactly the same thing: the app's own public origin, not anything Monobank-specific.
    IOptions<MonobankOptions> monobankOptions) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return BadRequest("A valid email is required.");
        }
        if (request.Password.Length < PasswordPolicy.MinLength)
        {
            return BadRequest($"Password must be at least {PasswordPolicy.MinLength} characters.");
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
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var user = await sender.Send(new GetCurrentUserQuery(User.GetUserId()), ct);
        return user is null ? Unauthorized() : Ok(user.ToDto());
    }

    [HttpPost("confirm-email")]
    [Authorize]
    public async Task<ActionResult<UserDto>> ConfirmEmail(ConfirmEmailRequest request, CancellationToken ct)
    {
        var user = await sender.Send(new ConfirmEmailCommand(User.GetUserId(), request.Code), ct);
        return user is null ? Unauthorized() : Ok(user.ToDto());
    }

    [HttpPost("resend-confirmation")]
    [Authorize]
    public async Task<IActionResult> ResendConfirmation(CancellationToken ct)
    {
        var user = await sender.Send(new ResendEmailConfirmationCommand(User.GetUserId()), ct);
        return user is null ? Unauthorized() : Ok();
    }

    // Always 200, whether or not the email is registered — never gives an enumeration signal.
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await sender.Send(new ForgotPasswordCommand(request.Email, monobankOptions.Value.PublicBaseUrl), ct);
        return Ok();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        await sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword), ct);
        return Ok();
    }
}
