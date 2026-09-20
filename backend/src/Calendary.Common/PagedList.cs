namespace Calendary.Common;

/// Generic paged-result shape for Application-layer list queries — mirrors the Api-layer
/// PagedResult&lt;T&gt; DTO (Application can't reference Calendary.Api.Dtos). Fully generic, no
/// entity dependency, so it belongs in Common rather than Application.
public record PagedList<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
