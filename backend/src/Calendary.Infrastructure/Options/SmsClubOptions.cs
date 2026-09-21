namespace Calendary.Infrastructure.Options;

public class SmsClubOptions
{
    public const string SectionName = "SmsClub";

    /// Bearer token from the SMS.Club dashboard (my.smsclub.mobi, Profile section). Empty in local
    /// dev always (#304). Present on staging too (#432 follow-up), but SmsClubService only uses it
    /// there when an admin has opted into AppSettings.RealIntegrationsOnStaging — otherwise it
    /// falls back to a fixed, unsent verification code ("0000"), since SMS.Club has no sandbox
    /// mode and every real send costs money and reaches a real phone. Production always uses the
    /// real value regardless of that flag.
    public string ApiKey { get; set; } = string.Empty;

    /// The registered SMS.Club sender name (alphaname) — already approved as "Calendary".
    public string SenderName { get; set; } = "Calendary";
}
