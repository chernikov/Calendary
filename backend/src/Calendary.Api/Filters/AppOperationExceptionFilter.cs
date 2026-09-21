using Calendary.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Calendary.Api.Filters;

/// Translates an AppOperationException thrown by an Application-layer handler into the same
/// response shape the old inline `BadRequest(string)`/`Conflict(string)` controller code used to
/// produce (an ObjectResult wrapping the plain message, content-negotiated to text/plain).
/// Applied via [TypeFilter] on individual controllers rather than global middleware, so it only
/// affects controllers that have actually moved their logic behind this exception convention.
public class AppOperationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AppOperationException ex) return;

        context.Result = new ObjectResult(ex.Message) { StatusCode = ex.StatusCode };
        context.ExceptionHandled = true;
    }
}
