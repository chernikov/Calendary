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
                "<p>Дякуємо за реєстрацію в Calendary. Можете одразу починати збирати свій календар.</p>");
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
}
