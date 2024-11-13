using Calendary.Api.Tools;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Calendary.Api.Controllers;

public abstract class BaseUserController : ControllerBase
{
    protected readonly IUserService _userService;

    protected AsyncLazy<User?> CurrentUser { get; private set; }

    public BaseUserController(IUserService userService)
    {
        _userService = userService;
        CurrentUser = new AsyncLazy<User?>(GetCurrentUserAsync);
    }

    protected async Task<User?> GetCurrentUserAsync()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

            var identity = Guid.Parse(jti!);
            var user = await _userService.GetUserByIdentityAsync(identity);
            return user;
        }
        return null;
    }
}
