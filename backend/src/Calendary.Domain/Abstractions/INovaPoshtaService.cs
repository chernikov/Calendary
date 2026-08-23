namespace Calendary.Domain.Abstractions;

public record NovaPoshtaWarehouse(string Number, string Address, string ClosesAt);

public interface INovaPoshtaService
{
    Task<IReadOnlyList<string>> SearchCitiesAsync(string query, CancellationToken ct = default);
    Task<IReadOnlyList<NovaPoshtaWarehouse>> GetWarehousesAsync(string city, CancellationToken ct = default);
}
