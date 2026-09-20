using System.ComponentModel.DataAnnotations;

namespace Calendary.Api.Dtos;

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record GoogleAuthRequest(string IdToken);
public record ConfirmEmailRequest(string Code);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
// #304: prefills the checkout form for returning customers — null until the user has completed
// checkout at least once.
public record LastDeliveryDto(string RecipientName, string Phone, string City, string WarehouseNumber, string WarehouseAddress);
// VerifiedPhone (normalized "+380..." or null): lets the checkout form skip re-verification when
// the prefilled/typed number already matches what was verified on a previous order (#304).
public record UserDto(Guid Id, string? DisplayName, string? Email, bool EmailConfirmed, string Role, LastDeliveryDto? LastDelivery, string? VerifiedPhone);
public record AuthResponse(string BearerToken, UserDto User);

public record PromptThemeDto(Guid Id, string Name, string Description, int SortOrder, IReadOnlyList<PromptDto> Prompts);
public record PromptDto(Guid Id, Guid PromptThemeId, string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder);
public record ImageStyleDto(Guid Id, string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder);
public record PromptLibraryDto(IReadOnlyList<PromptThemeDto> Themes, IReadOnlyList<ImageStyleDto> Styles);

public record SheetPlanItem(int Index, Guid PromptId, Guid ImageStyleId, Guid? PhotoId);
public record SaveSheetPlanRequest(IReadOnlyList<SheetPlanItem> Items);

// #304: cheap structural checks via [ApiController]'s automatic 400 — the deeper business-rule
// validation (label sanitization) still lives in AddPersonalDateCommandHandler.
public record AddPersonalDateRequest(
    [Range(1, 31)] int Day,
    [Range(1, 12)] int Month,
    // 22 mirrors OrderAccess.MaxLabelLength — kept as a literal here since Dtos.cs doesn't
    // otherwise reference Calendary.Application, and DataAnnotations need a compile-time constant.
    [Required, StringLength(22, MinimumLength = 1)] string Label);
public record PersonalDateDto(Guid Id, int Day, int Month, string Label);

public record SheetVariantDto(Guid Id, string ImageUrl, DateTime CreatedAtUtc, decimal? CostUsd);

public record SheetDto(
    Guid Id, string Kind, int Index, string Status, bool IsSelected, string? ImageUrl,
    Guid? PromptId, string? PromptName, Guid? ImageStyleId, string? ImageStyleName,
    Guid? PhotoId, Guid? ActiveVariantId, IReadOnlyList<SheetVariantDto> Variants, string? FailureReason);
// #303's lightweight polling shape — status per sheet only, no ImageUrl/prompt/style, for the
// "generating"/"month" pages' ~1.5s poll instead of re-fetching the full OrderDto every tick.
public record SheetProgressDto(int Index, string Kind, string Status, string? FailureReason);
public record OrderProgressDto(Guid Id, string Status, DateTime StatusUpdatedAtUtc, IReadOnlyList<SheetProgressDto> Sheets);

public record ConfirmCoverRequest(Guid SheetId);

// #304: cheap structural checks (non-empty) via [ApiController]'s automatic 400. Phone-format
// normalization and Nova-Poshta city/warehouse-existence checks need async lookups DataAnnotations
// can't express — those stay in OrderAccess.ValidateAndNormalizeDeliveryAsync.
public record CheckoutRequest(
    [Required] string RecipientName,
    [Required] string Phone,
    [Required] string City,
    [Required] string WarehouseNumber,
    [Required] string WarehouseAddress);
public record DeliveryDto(string RecipientName, string Phone, string City, string WarehouseNumber, string WarehouseAddress, string? TrackingNumber);

// #304: phone verification at checkout, scoped to the user (see SendPhoneVerificationCommand).
public record SendPhoneVerificationRequest([Required] string Phone);
public record ConfirmPhoneVerificationRequest([Required] string Code);

public record SetPrintQuantityRequest(int Quantity);

public record BatchCheckoutRequest(
    [Required, MinLength(1)] IReadOnlyList<Guid> OrderIds,
    [Required] string RecipientName,
    [Required] string Phone,
    [Required] string City,
    [Required] string WarehouseNumber,
    [Required] string WarehouseAddress);

public record PayResponseDto(string PageUrl);
public record PaymentDto(string Method, string Status, decimal Amount, DateTime? PaidAtUtc);

public record NovaPoshtaWarehouseDto(string Number, string Address, string ClosesAt, bool IsPostomat);

public record OrderPhotoDto(Guid Id, string Url, string ThumbUrl);

public record OrderDto(
    Guid Id,
    string Status,
    IReadOnlyList<OrderPhotoDto> Photos,
    decimal Price,
    int PrintQuantity,
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
    string WeekStart,
    string? PromoCode,
    decimal DiscountAmount
);

public record OrderSummaryDto(
    Guid Id,
    string Status,
    decimal Price,
    int PrintQuantity,
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

public record ProductSettingsDto(decimal BasePrice);
public record SetProductSettingsRequest(decimal BasePrice);

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

public record ApplyPromoCodeRequest(string Code);

public record PromoCodeDto(
    Guid Id, string Code, string Type, decimal Value,
    DateTime? ValidFromUtc, DateTime? ValidToUtc,
    int? MaxRedemptions, int RedemptionsUsed, decimal? MinOrderAmount, bool IsActive);

public record SavePromoCodeRequest(
    string Code, string Type, decimal Value,
    DateTime? ValidFromUtc, DateTime? ValidToUtc,
    int? MaxRedemptions, decimal? MinOrderAmount, bool IsActive);
