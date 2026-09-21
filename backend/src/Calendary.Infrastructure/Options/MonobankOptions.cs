namespace Calendary.Infrastructure.Options;

public class MonobankOptions
{
    public const string SectionName = "Monobank";

    public string MerchantToken { get; set; } = string.Empty;

    /// The app's own public origin (e.g. https://calendary.com.ua), used to build the absolute
    /// redirectUrl/webHookUrl Monobank needs — both frontend and API are served from this same
    /// origin behind Caddy/nginx (see CLAUDE.md's Deployment section). Empty in local dev, where
    /// MerchantToken is also unset and the fallback path never calls out to Monobank anyway.
    public string PublicBaseUrl { get; set; } = string.Empty;
}
