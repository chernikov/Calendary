using Calendary.Api.Auth;
using Calendary.Api.Dtos;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IDevAuthService authService, AppDbContext db) : ControllerBase
{
    /// Dev-mode passwordless "start": in a real deployment this would email/text a link.
    /// Here the token is simply returned to the caller so the frontend can present the
    /// "link sent" screen and then act as if the user tapped the link.
    [HttpPost("start")]
    [AllowAnonymous]
    public async Task<ActionResult<StartAuthResponse>> Start(StartAuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Contact))
        {
            return BadRequest("Contact (email or phone) is required.");
        }

        var result = await authService.StartAsync(request.Contact.Trim());
        return Ok(new StartAuthResponse(result.Token, result.Contact));
    }

    [HttpPost("complete")]
    [AllowAnonymous]
    public async Task<ActionResult<CompleteAuthResponse>> Complete(CompleteAuthRequest request)
    {
        var result = await authService.CompleteAsync(request.Token);
        if (result is null)
        {
            return NotFound("Token is invalid or already used.");
        }

        var (bearer, user) = result.Value;
        return Ok(new CompleteAuthResponse(bearer, user.ToDto()));
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
