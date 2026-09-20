using System.Text.RegularExpressions;

namespace Calendary.Common;

/// Accepts the common ways a customer might type a Ukrainian number — "+380671234567",
/// "380671234567", "0671234567", "067 123 45 67", "+38 (067) 123-45-67" — and normalizes them all
/// to the canonical "+380XXXXXXXXX" shape (#304), or returns null if the digits don't fit any of
/// those patterns.
public static partial class UkrainianPhoneNumber
{
    [GeneratedRegex(@"^\+380\d{9}$")]
    private static partial Regex CanonicalForm();

    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        var normalized = digits.Length switch
        {
            12 when digits.StartsWith("380") => "+" + digits,
            10 when digits.StartsWith("0") => "+380" + digits[1..],
            9 => "+380" + digits,
            _ => null,
        };

        return normalized is not null && CanonicalForm().IsMatch(normalized) ? normalized : null;
    }
}
