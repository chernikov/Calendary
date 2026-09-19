using System.Security.Claims;
using Calendary.Domain.Enums;

namespace Calendary.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("No authenticated user on this request.");
        return Guid.Parse(raw);
    }

    public static UserRole GetRole(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("No role claim on this request.");
        return Enum.Parse<UserRole>(raw);
    }
}
