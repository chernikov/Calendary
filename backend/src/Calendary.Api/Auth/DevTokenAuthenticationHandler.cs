using System.Security.Claims;
using System.Text.Encodings.Web;
using Calendary.Domain.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Calendary.Api.Auth;

public static class DevTokenAuth
{
    public const string Scheme = "DevToken";
}

public class DevTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IDevAuthService authService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        var header = headerValue.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = header["Bearer ".Length..].Trim();
        var user = await authService.ResolveBearerTokenAsync(token);
        if (user is null)
        {
            return AuthenticateResult.Fail("Invalid or expired session token");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email ?? user.Phone ?? user.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, DevTokenAuth.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, DevTokenAuth.Scheme);
        return AuthenticateResult.Success(ticket);
    }
}
