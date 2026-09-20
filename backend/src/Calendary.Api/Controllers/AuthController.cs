using Calendary.Api.Auth;
using Calendary.Api.Dtos;
using Calendary.Api.Filters;
using Calendary.Application.Auth;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/auth")]
[TypeFilter(typeof(AppOperationExceptionFilter))]
public class AuthController(
    IPasswordAuthService passwordAuth,
    IGoogleAuthService googleAuth,
    ISessionTokenService sessionTokens,
    IEmailService email,
    ILogger<AuthController> logger,
    ISender sender) : ControllerBase
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
}
