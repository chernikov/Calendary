namespace Calendary.Application.Orders;

/// Small cross-layer records used by the Orders commands/queries below — Application can't
/// reference Calendary.Api.Dtos, so these are the Application-facing equivalents of a few request
/// DTOs and one list-view projection. Mirrors how PaymentInvoice already lives as a plain record
/// next to IPaymentService.
public record OrderSummary(
    Guid Id, string Status, decimal Price, int PrintQuantity,
    DateTime CreatedAtUtc, DateTime StatusUpdatedAtUtc,
    string? StyleName, string? CoverImageUrl, bool IsArchived);

public record SheetPlanEntry(int Index, Guid PromptId, Guid ImageStyleId, Guid? PhotoId);

// #303's lightweight polling shape — status per sheet only, no ImageUrl/prompt/style payload, so
// the "generating"/"month" pages' ~1.5s poll doesn't re-fetch the full sheet graph every tick.
public record SheetProgress(int Index, string Kind, string Status, string? FailureReason);

public record OrderProgress(Guid Id, string Status, DateTime StatusUpdatedAtUtc, IReadOnlyList<SheetProgress> Sheets);

public record DeliveryInfo(
    string RecipientName, string Phone, string City, string WarehouseNumber, string WarehouseAddress);
