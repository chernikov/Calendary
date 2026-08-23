using Calendary.Domain.Abstractions;

namespace Calendary.Infrastructure.Services;

/// Mock Nova Poshta lookup: a small static dataset standing in for the real branch API.
public class MockNovaPoshtaService : INovaPoshtaService
{
    private static readonly string[] Cities =
    [
        "Львів", "Київ", "Харків", "Одеса", "Дніпро", "Вінниця", "Івано-Франківськ", "Тернопіль"
    ];

    private static readonly Dictionary<string, NovaPoshtaWarehouse[]> Warehouses = new()
    {
        ["Львів"] =
        [
            new("№12", "вул. Городоцька, 359", "до 20:00"),
            new("№34", "вул. Липинського, 54", "до 21:00"),
            new("№81", "пр. Червоної Калини, 62", "до 20:00")
        ],
        ["Київ"] =
        [
            new("№1", "вул. Хрещатик, 22", "до 22:00"),
            new("№47", "просп. Перемоги, 100", "до 21:00"),
            new("№103", "вул. Драгоманова, 14", "до 20:00")
        ]
    };

    private static readonly NovaPoshtaWarehouse[] DefaultWarehouses =
    [
        new("№1", "центральне відділення", "до 20:00"),
        new("№5", "вул. Соборна, 10", "до 20:00"),
        new("№18", "вул. Незалежності, 3", "до 19:00")
    ];

    public Task<IReadOnlyList<string>> SearchCitiesAsync(string query, CancellationToken ct = default)
    {
        IReadOnlyList<string> result = Cities
            .Where(c => string.IsNullOrWhiteSpace(query) || c.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<NovaPoshtaWarehouse>> GetWarehousesAsync(string city, CancellationToken ct = default)
    {
        IReadOnlyList<NovaPoshtaWarehouse> result = Warehouses.TryGetValue(city, out var w) ? w : DefaultWarehouses;
        return Task.FromResult(result);
    }
}
