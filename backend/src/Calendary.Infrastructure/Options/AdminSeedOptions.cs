namespace Calendary.Infrastructure.Options;

public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = "admin@calendary.com.ua";

    /// Left empty, seeding is skipped (logged) rather than creating an unusable/insecure account.
    public string Password { get; set; } = string.Empty;
}
