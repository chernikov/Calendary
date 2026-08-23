namespace Calendary.Infrastructure.Options;

public class ResendOptions
{
    public const string SectionName = "Resend";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@calendary.com.ua";
    public string FromName { get; set; } = "Calendary";
}
