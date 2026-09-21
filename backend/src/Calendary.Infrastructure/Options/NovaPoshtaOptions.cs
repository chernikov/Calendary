namespace Calendary.Infrastructure.Options;

public class NovaPoshtaOptions
{
    public const string SectionName = "NovaPoshta";

    public string ApiKey { get; set; } = string.Empty;

    // #432: the app's own registered sender (a private-person Nova Poshta account, confirmed
    // working against the live InternetDocument/save API with no business contract required — see
    // #432's issue body). Blank in local/staging by design (see NovaPoshtaService.CreateShipmentAsync)
    // — only prod carries real values, same fallback pattern as Monobank/SmsClub.
    public string SenderCounterpartyRef { get; set; } = string.Empty;
    public string SenderContactRef { get; set; } = string.Empty;
    public string SenderCityRef { get; set; } = string.Empty;
    public string SenderWarehouseRef { get; set; } = string.Empty;
    public string SendersPhone { get; set; } = string.Empty;
}
