namespace Calendary.Api.Dtos;

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record GoogleAuthRequest(string IdToken);
public record ConfirmEmailRequest(string Code);
public record UserDto(Guid Id, string? DisplayName, string? Email, bool EmailConfirmed, string Role);
public record AuthResponse(string BearerToken, UserDto User);

public record PromptThemeDto(Guid Id, string Name, string Description, int SortOrder, IReadOnlyList<PromptDto> Prompts);
public record PromptDto(Guid Id, Guid PromptThemeId, string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder);
public record ImageStyleDto(Guid Id, string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder);
public record PromptLibraryDto(IReadOnlyList<PromptThemeDto> Themes, IReadOnlyList<ImageStyleDto> Styles);

public record SheetPlanItem(int Index, Guid PromptId, Guid ImageStyleId, Guid? PhotoId);
public record SaveSheetPlanRequest(IReadOnlyList<SheetPlanItem> Items);

public record AddPersonalDateRequest(int Day, int Month, string Label);
public record PersonalDateDto(Guid Id, int Day, int Month, string Label);

public record SheetVariantDto(Guid Id, string ImageUrl, DateTime CreatedAtUtc, decimal? CostUsd);

public record SheetDto(
    Guid Id, string Kind, int Index, string Status, bool IsSelected, string? ImageUrl,
    Guid? PromptId, string? PromptName, Guid? ImageStyleId, string? ImageStyleName,
    Guid? PhotoId, Guid? ActiveVariantId, IReadOnlyList<SheetVariantDto> Variants);
public record ConfirmCoverRequest(Guid SheetId);

public record CheckoutRequest(string RecipientName, string Phone, string City, string WarehouseNumber, string WarehouseAddress);
public record DeliveryDto(string RecipientName, string Phone, string City, string WarehouseNumber, string WarehouseAddress, string? TrackingNumber);

public record PayRequest(string Method);
public record PaymentDto(string Method, string Status, decimal Amount, DateTime? PaidAtUtc);

public record NovaPoshtaWarehouseDto(string Number, string Address, string ClosesAt);

public record OrderPhotoDto(Guid Id, string Url, string ThumbUrl);

public record OrderDto(
    Guid Id,
    string Status,
    IReadOnlyList<OrderPhotoDto> Photos,
    decimal Price,
    int RegenerationsRemaining,
    decimal TotalGenerationCostUsd,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    bool IsExpired,
    IReadOnlyList<PersonalDateDto> PersonalDates,
    IReadOnlyList<SheetDto> Sheets,
    PaymentDto? Payment,
    DeliveryDto? Delivery,
    IReadOnlyList<string> HolidayCountries,
    string WeekStart
);

public record OrderSummaryDto(
    Guid Id,
    string Status,
    decimal Price,
    DateTime CreatedAtUtc,
    DateTime StatusUpdatedAtUtc,
    string? StyleName,
    string? CoverImageUrl,
    bool IsArchived);

// — Admin —
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public record AdminOrderSummaryDto(
    Guid Id, string Status, Guid UserId, string? UserEmail, string? UserDisplayName,
    decimal Price, decimal TotalGenerationCostUsd, DateTime CreatedAtUtc, DateTime StatusUpdatedAtUtc);

public record AdminUserDto(
    Guid Id, string? Email, string? DisplayName, string Role, string AuthProvider,
    bool EmailConfirmed, DateTime CreatedAtUtc, int OrderCount);

public record SetImageGenerationProviderRequest(string Provider);
public record ImageGenerationProviderDto(string Provider);

// Presence-only — never expose the actual key/secret values to the admin UI.
public record ConfigStatusDto(
    bool OpenAiConfigured,
    bool GeminiConfigured,
    bool GoogleConfigured,
    bool ResendConfigured,
    bool MonobankConfigured);

public record BackupSnapshotDto(DateTime TimeUtc, IReadOnlyList<string> Tags);
public record BackupStatusDto(bool Configured, IReadOnlyList<BackupSnapshotDto> Snapshots);

public record SavePromptThemeRequest(string Name, string Description, int SortOrder);
public record SavePromptRequest(Guid PromptThemeId, string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder);
public record SaveImageStyleRequest(string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder);

public record GenerateSheetRequest(Guid PromptId, Guid ImageStyleId, Guid? PhotoId);

public record HolidayDto(Guid Id, string Country, int Year, int Day, int Month, string Name, string ShortName);
public record SaveHolidayRequest(string Country, int Year, int Day, int Month, string Name, string ShortName);

public record SaveHolidaySettingsRequest(IReadOnlyList<string> Countries, string WeekStart);
