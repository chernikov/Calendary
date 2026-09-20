using System.Security.Claims;
using Calendary.Api.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Calendary.Api.Filters;

/// Simple "who did what to which resource, when" audit log for AdminController (#300) — GET
/// actions aren't logged (read-only, nothing to audit); request *bodies* aren't logged either (one
/// action accepts a raw photo upload, and most of the rest carry full entity text that isn't
/// interesting for an audit trail) — just the admin's identity, the action, and route parameters,
/// which already cover every resource id in play (orderId, themeId, promoCodeId, ...).
public class AdminAuditLogFilter(ILogger<AdminAuditLogFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        if (HttpMethods.IsGet(context.HttpContext.Request.Method))
        {
            return;
        }

        var admin = context.HttpContext.User;
        var adminId = admin.GetUserId();
        var adminName = admin.FindFirstValue(ClaimTypes.Name) ?? adminId.ToString();
        var routeValues = string.Join(", ", context.RouteData.Values
            .Where(kv => kv.Key != "controller" && kv.Key != "action")
            .Select(kv => $"{kv.Key}={kv.Value}"));
        var outcome = result.Exception is null ? "ok" : "threw " + result.Exception.GetType().Name;

        logger.LogInformation(
            "Admin audit: {AdminName} ({AdminId}) {Method} {Action}({RouteValues}) -> {Outcome}",
            adminName, adminId, context.HttpContext.Request.Method, context.ActionDescriptor.RouteValues["action"],
            routeValues, outcome);
    }
}
