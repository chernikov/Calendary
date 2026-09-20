namespace Calendary.Infrastructure.Options;

public class SmsClubOptions
{
    public const string SectionName = "SmsClub";

    /// Bearer token from the SMS.Club dashboard (my.smsclub.mobi, Profile section). Empty in local
    /// dev and staging on purpose (#304) — SmsClubService falls back to a fixed, unsent
    /// verification code ("0000") when this is blank, since SMS.Club has no sandbox mode and every
    /// real send costs money and reaches a real phone. Only prod gets the real value.
    public string ApiKey { get; set; } = string.Empty;

    /// The registered SMS.Club sender name (alphaname) — already approved as "Calendary".
    public string SenderName { get; set; } = "Calendary";
}
