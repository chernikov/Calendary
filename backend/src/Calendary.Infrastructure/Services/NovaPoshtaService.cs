using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

/// Calls Nova Poshta's public Address API (https://api.novaposhta.ua/v2.0/json/) directly over
/// HTTP — same minimal-deps approach as ResendEmailService. Falls back to the small static
/// dataset MockNovaPoshtaService used to carry when no API key is configured, so local dev keeps
/// working without one.
public class NovaPoshtaService(HttpClient httpClient, IOptions<NovaPoshtaOptions> options, ILogger<NovaPoshtaService> logger)
    : INovaPoshtaService
{
    private const string ApiUrl = "https://api.novaposhta.ua/v2.0/json/";
    private readonly NovaPoshtaOptions _options = options.Value;

    // System.Text.Json's default encoder escapes non-ASCII characters as \uXXXX — Nova Poshta's
    // backend silently fails to match FindByString against an escaped Cyrillic value (returns the
    // *entire* unfiltered city/warehouse dictionary instead of an error), so city/warehouse
    // filters must be sent as raw UTF-8 text. Confirmed directly against the real API.
    private static readonly JsonSerializerOptions RequestJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly string[] FallbackCities =
    [
        "Львів", "Київ", "Харків", "Одеса", "Дніпро", "Вінниця", "Івано-Франківськ", "Тернопіль"
    ];

    // Ref/CityRef are Guid.Empty — fallback data is display-only for local dev with no API key,
    // never fed into CreateShipmentAsync (which requires a configured ApiKey to run at all).
    private static readonly Dictionary<string, NovaPoshtaWarehouse[]> FallbackWarehouses = new()
    {
        ["Львів"] =
        [
            new("№12", "вул. Городоцька, 359", "до 20:00", IsPostomat: false, Guid.Empty, Guid.Empty),
            new("№34", "вул. Липинського, 54", "до 21:00", IsPostomat: false, Guid.Empty, Guid.Empty),
            new("№81", "пр. Червоної Калини, 62", "до 20:00", IsPostomat: true, Guid.Empty, Guid.Empty)
        ],
        ["Київ"] =
        [
            new("№1", "вул. Хрещатик, 22", "до 22:00", IsPostomat: false, Guid.Empty, Guid.Empty),
            new("№47", "просп. Перемоги, 100", "до 21:00", IsPostomat: false, Guid.Empty, Guid.Empty),
            new("№103", "вул. Драгоманова, 14", "до 20:00", IsPostomat: true, Guid.Empty, Guid.Empty)
        ]
    };

    private static readonly NovaPoshtaWarehouse[] DefaultFallbackWarehouses =
    [
        new("№1", "центральне відділення", "до 20:00", IsPostomat: false, Guid.Empty, Guid.Empty),
        new("№5", "вул. Соборна, 10", "до 20:00", IsPostomat: false, Guid.Empty, Guid.Empty),
        new("№18", "вул. Незалежності, 3", "до 19:00", IsPostomat: true, Guid.Empty, Guid.Empty)
    ];

    public async Task<IReadOnlyList<string>> SearchCitiesAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return FallbackCities
                .Where(c => string.IsNullOrWhiteSpace(query) || c.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var data = await CallAsync("getCities", new { FindByString = query, Limit = "20" }, ct);
        if (data is null) return [];

        return data
            .Select(item => item.TryGetProperty("Description", out var d) ? d.GetString() : null)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();
    }

    public async Task<IReadOnlyList<NovaPoshtaWarehouse>> GetWarehousesAsync(string city, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return FallbackWarehouses.TryGetValue(city, out var w) ? w : DefaultFallbackWarehouses;
        }

        var data = await CallAsync("getWarehouses", new { CityName = city, Limit = "50" }, ct);
        if (data is null) return [];

        return data
            .Select(item =>
            {
                var number = item.TryGetProperty("Number", out var n) ? n.GetString() : null;
                var address = item.TryGetProperty("ShortAddress", out var a) && !string.IsNullOrWhiteSpace(a.GetString())
                    ? a.GetString()
                    : item.TryGetProperty("Description", out var d) ? d.GetString() : null;
                var isPostomat = item.TryGetProperty("CategoryOfWarehouse", out var cat)
                    && string.Equals(cat.GetString(), "Postomat", StringComparison.OrdinalIgnoreCase);
                var warehouseRef = item.TryGetProperty("Ref", out var r) && Guid.TryParse(r.GetString(), out var wr) ? wr : Guid.Empty;
                var cityRef = item.TryGetProperty("CityRef", out var cr) && Guid.TryParse(cr.GetString(), out var cref) ? cref : Guid.Empty;
                return new NovaPoshtaWarehouse(number ?? "?", address ?? "", ParseClosesAt(item), isPostomat, warehouseRef, cityRef);
            })
            .ToList();
    }

    private Task<List<JsonElement>?> CallAsync(string method, object methodProperties, CancellationToken ct) =>
        CallAsync("Address", method, methodProperties, ct);

    private async Task<List<JsonElement>?> CallAsync(string modelName, string method, object methodProperties, CancellationToken ct)
    {
        try
        {
            var requestJson = JsonSerializer.Serialize(
                new { apiKey = _options.ApiKey, modelName, calledMethod = method, methodProperties },
                RequestJsonOptions);
            using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(ApiUrl, content, ct);

            var envelope = await response.Content.ReadFromJsonAsync<NovaPoshtaResponse>(cancellationToken: ct);
            if (envelope is null || !envelope.Success)
            {
                logger.LogWarning(
                    "Nova Poshta {Model}/{Method} failed: {Errors}",
                    modelName, method,
                    envelope?.Errors is { Count: > 0 } errors ? string.Join("; ", errors) : "unknown response shape");
                return null;
            }
            return envelope.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nova Poshta {Model}/{Method} threw", modelName, method);
            return null;
        }
    }

    // #432: real Nova Poshta express waybill creation. Falls back to a fake tracking number when
    // no sender is configured (local dev / staging by deliberate design — see NovaPoshtaOptions —
    // only prod carries real SenderCounterpartyRef etc.), same pattern as
    // MonobankPaymentService/SmsClubService. Throws AppOperationException(502) on any real API
    // failure rather than returning null/empty, since the caller (AdvanceOrderFulfillmentCommand)
    // needs to surface a clear error to the admin instead of silently leaving TrackingNumber unset.
    public async Task<NovaPoshtaShipmentResult> CreateShipmentAsync(NovaPoshtaShipmentRecipient recipient, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.SenderCounterpartyRef))
        {
            return new NovaPoshtaShipmentResult(
                $"2040{Random.Shared.Next(1000000, 9999999)}", 0m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));
        }

        var recipientCounterparty = await CallAsync("Counterparty", "save", new
        {
            CounterpartyProperty = "Recipient",
            CounterpartyType = "PrivatePerson",
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            Phone = recipient.Phone,
            Email = "",
        }, ct);

        var recipientData = recipientCounterparty?.FirstOrDefault();
        if (recipientData is null
            || !recipientData.Value.TryGetProperty("Ref", out var recipientRefProp)
            || !Guid.TryParse(recipientRefProp.GetString(), out var recipientRef)
            || !recipientData.Value.TryGetProperty("ContactPerson", out var contactPersonEnvelope)
            || !contactPersonEnvelope.TryGetProperty("data", out var contactPersonData)
            || contactPersonData.GetArrayLength() == 0
            || !contactPersonData[0].TryGetProperty("Ref", out var contactRefProp)
            || !Guid.TryParse(contactRefProp.GetString(), out var contactRecipientRef))
        {
            throw new AppOperationException("Не вдалося зареєструвати отримувача в Новій Пошті.", 502);
        }

        var document = await CallAsync("InternetDocument", "save", new
        {
            PayerType = "Sender",
            PaymentMethod = "Cash",
            DateTime = DateTime.UtcNow.ToString("dd.MM.yyyy"),
            CargoType = "Parcel",
            VolumeGeneral = "0.0004",
            Weight = "1",
            ServiceType = "WarehouseWarehouse",
            SeatsAmount = "1",
            Description = "Фотокалендар",
            Cost = "300",
            CitySender = _options.SenderCityRef,
            Sender = _options.SenderCounterpartyRef,
            SenderAddress = _options.SenderWarehouseRef,
            ContactSender = _options.SenderContactRef,
            SendersPhone = _options.SendersPhone,
            CityRecipient = recipient.CityRef.ToString(),
            Recipient = recipientRef.ToString(),
            RecipientAddress = recipient.WarehouseRef.ToString(),
            ContactRecipient = contactRecipientRef.ToString(),
            RecipientsPhone = recipient.Phone,
        }, ct);

        var documentData = document?.FirstOrDefault();
        if (documentData is null
            || !documentData.Value.TryGetProperty("IntDocNumber", out var trackingProp)
            || string.IsNullOrWhiteSpace(trackingProp.GetString()))
        {
            throw new AppOperationException("Не вдалося створити накладну в Новій Пошті.", 502);
        }

        var cost = documentData.Value.TryGetProperty("CostOnSite", out var costProp) && costProp.TryGetDecimal(out var c) ? c : 0m;
        var estimatedDelivery = documentData.Value.TryGetProperty("EstimatedDeliveryDate", out var dateProp)
            && DateOnly.TryParseExact(dateProp.GetString(), "dd.MM.yyyy", out var d)
            ? d
            : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));

        return new NovaPoshtaShipmentResult(trackingProp.GetString()!, cost, estimatedDelivery);
    }

    // Confirmed directly against a live getWarehouses response: Schedule keys are English day
    // names ("Monday".."Sunday"), not the previously-guessed weekday-number shape.
    private static readonly string[] DayNames =
    [
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ];

    private static string ParseClosesAt(JsonElement item)
    {
        if (item.TryGetProperty("Schedule", out var schedule) && schedule.ValueKind == JsonValueKind.Object)
        {
            var todayKey = DayNames[(int)DateTime.Now.DayOfWeek];
            if (schedule.TryGetProperty(todayKey, out var hours) && hours.ValueKind == JsonValueKind.String)
            {
                var text = hours.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var closing = text.Split('-').LastOrDefault()?.Trim();
                    return string.IsNullOrWhiteSpace(closing) ? text : $"до {closing}";
                }
            }
        }
        return "Див. розклад на сайті Нової Пошти";
    }

    private record NovaPoshtaResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("data")] List<JsonElement>? Data,
        [property: JsonPropertyName("errors")] List<string>? Errors);
}
