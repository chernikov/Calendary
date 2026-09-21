namespace Calendary.Infrastructure.Options;

public class NovaPoshtaOptions
{
    public const string SectionName = "NovaPoshta";

    public string ApiKey { get; set; } = string.Empty;

    // #432: the app's own registered sender (a private-person Nova Poshta account, confirmed
    // working against the live InternetDocument/save API with no business contract required — see
    // #432's issue body). Blank in local dev always. Present on staging too (#432 follow-up), but
    // NovaPoshtaService.CreateShipmentAsync only uses these there when an admin has opted into
    // AppSettings.RealIntegrationsOnStaging — otherwise it falls back to a fake tracking number.
    // Production always uses the real values regardless of that flag.
    public string SenderCounterpartyRef { get; set; } = string.Empty;
    public string SenderContactRef { get; set; } = string.Empty;
    public string SenderCityRef { get; set; } = string.Empty;
    public string SenderWarehouseRef { get; set; } = string.Empty;
    public string SendersPhone { get; set; } = string.Empty;
}
