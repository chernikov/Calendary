namespace Calendary.Domain.Entities;

public static class EmailConfirmationCodeGenerator
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

    public static string Generate() => Random.Shared.Next(0, 1_000_000).ToString("D6");
}
