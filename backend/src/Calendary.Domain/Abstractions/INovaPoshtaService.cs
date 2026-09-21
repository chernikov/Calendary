namespace Calendary.Domain.Abstractions;

public record NovaPoshtaWarehouse(string Number, string Address, string ClosesAt, bool IsPostomat, Guid Ref, Guid CityRef);

/// #432: everything InternetDocument/save needs about the recipient side — CityRef/WarehouseRef
/// come from the matched NovaPoshtaWarehouse captured at checkout (OrderAccess.
/// ValidateAndNormalizeDeliveryAsync), not looked up again at shipment time.
public record NovaPoshtaShipmentRecipient(string FirstName, string LastName, string Phone, Guid CityRef, Guid WarehouseRef);

public record NovaPoshtaShipmentResult(string TrackingNumber, decimal CostOnSite, DateOnly EstimatedDeliveryDate);

public interface INovaPoshtaService
{
    Task<IReadOnlyList<string>> SearchCitiesAsync(string query, CancellationToken ct = default);
    Task<IReadOnlyList<NovaPoshtaWarehouse>> GetWarehousesAsync(string city, CancellationToken ct = default);

    /// Creates a real Nova Poshta express waybill (#432) and returns its tracking number. Throws
    /// AppOperationException(502) on any Nova Poshta API failure — callers must not let this crash
    /// the whole request (see AdvanceOrderFulfillmentCommand).
    Task<NovaPoshtaShipmentResult> CreateShipmentAsync(NovaPoshtaShipmentRecipient recipient, CancellationToken ct = default);
}
