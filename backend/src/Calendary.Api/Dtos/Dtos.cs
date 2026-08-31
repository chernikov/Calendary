namespace Calendary.Api.Dtos;

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record GoogleAuthRequest(string IdToken);
public record ConfirmEmailRequest(string Code);
public record UserDto(Guid Id, string? DisplayName, string? Email, bool EmailConfirmed, string Role);
public record AuthResponse(string BearerToken, UserDto User);

public record StyleCategoryDto(Guid Id, string Code, string Name, string Description, int SortOrder);

public record SelectStyleRequest(Guid StyleCategoryId);
public record AddPersonalDateRequest(int Day, int Month, string Label);
public record PersonalDateDto(Guid Id, int Day, int Month, string Label);

public record SheetDto(Guid Id, string Kind, int Index, string Status, bool IsSelected, string? ImageUrl, int VariantCount);
public record ConfirmCoverRequest(Guid SheetId);

public record CheckoutRequest(string RecipientName, string Phone, string City, string WarehouseNumber, string WarehouseAddress);
public record DeliveryDto(string RecipientName, string Phone, string City, string WarehouseNumber, string WarehouseAddress, string? TrackingNumber);

public record PayRequest(string Method);
public record PaymentDto(string Method, string Status, decimal Amount, DateTime? PaidAtUtc);

public record NovaPoshtaWarehouseDto(string Number, string Address, string ClosesAt);

public record OrderDto(
    Guid Id,
    string Status,
    string? PhotoUrl,
    StyleCategoryDto? StyleCategory,
    decimal Price,
    int RegenerationsRemaining,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    bool IsExpired,
    IReadOnlyList<PersonalDateDto> PersonalDates,
    IReadOnlyList<SheetDto> Sheets,
    PaymentDto? Payment,
    DeliveryDto? Delivery
);

// — Admin —
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public record AdminOrderSummaryDto(
    Guid Id, string Status, Guid UserId, string? UserEmail, string? UserDisplayName,
    decimal Price, DateTime CreatedAtUtc, DateTime StatusUpdatedAtUtc);

public record AdminUserDto(
    Guid Id, string? Email, string? DisplayName, string Role, string AuthProvider,
    bool EmailConfirmed, DateTime CreatedAtUtc, int OrderCount);

public record SetImageGenerationProviderRequest(string Provider);
public record ImageGenerationProviderDto(string Provider);
