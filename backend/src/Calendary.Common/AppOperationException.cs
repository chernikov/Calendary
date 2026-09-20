namespace Calendary.Common;

/// Thrown by an Application-layer command/query handler to signal a validation (400) or conflict
/// (409) failure — the counterpart to a plain `null` return, which means "not found or not owned".
/// A controller-scoped exception filter (not global middleware) catches this and reproduces the
/// exact response shape the old inline `BadRequest(string)`/`Conflict(string)` calls used to
/// produce. Generic on purpose (not order-specific) so any future Application feature can reuse it.
public class AppOperationException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode => statusCode;
}
