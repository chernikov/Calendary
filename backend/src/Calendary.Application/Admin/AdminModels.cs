namespace Calendary.Application.Admin;

/// Small cross-layer projection records for admin list queries — Application can't reference
/// Calendary.Api.Dtos, same reasoning as Orders/OrderModels.cs's OrderSummary.
public record AdminOrderSummary(
    Guid Id, string Status, Guid UserId, string? UserEmail, string? UserDisplayName,
    decimal Price, decimal TotalGenerationCostUsd, DateTime CreatedAtUtc, DateTime StatusUpdatedAtUtc,
    string? TrackingNumber);

public record AdminUserSummary(
    Guid Id, string? Email, string? DisplayName, string Role, string AuthProvider,
    bool EmailConfirmed, DateTime CreatedAtUtc, int OrderCount);

public record AdminOrderStatusHistoryEntry(string? FromStatus, string ToStatus, DateTime ChangedAtUtc);
