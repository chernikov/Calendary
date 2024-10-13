using Calendary.Api.Tools;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Calendary.Api.Controllers;

public abstract class BaseUserController : Controller
{
    private readonly IUserService _userService;

    protected AsyncLazy<User?> CurrentUser { get; private set; }

    public BaseUserController(IUserService userService)
    {
        _userService = userService;
        CurrentUser = new AsyncLazy<User?>(GetCurrentUserAsync);
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userService.GetUserByEmailAsync(email!);
            return user;
        }
        return null;
    }
}
