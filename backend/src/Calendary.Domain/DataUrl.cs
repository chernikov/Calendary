namespace Calendary.Domain;

/// Parsing/building of "data:image/png;base64,AAAA..." strings. Calendary.AI keeps its own
/// internal copy on purpose — that project deliberately references none of the others.
public static class DataUrl
{
    public static bool TryParse(string? dataUrl, out string mimeType, out byte[] bytes)
    {
        mimeType = string.Empty;
        bytes = [];

        if (string.IsNullOrWhiteSpace(dataUrl) || !dataUrl.StartsWith("data:", StringComparison.Ordinal))
        {
            return false;
        }

        var comma = dataUrl.IndexOf(',');
        if (comma < 0)
        {
            return false;
        }

        var header = dataUrl[5..comma]; // "image/png;base64"
        if (!header.Contains("base64", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            bytes = Convert.FromBase64String(dataUrl[(comma + 1)..]);
        }
        catch (FormatException)
        {
            return false;
        }

        mimeType = header.Split(';')[0].Trim().ToLowerInvariant();
        return mimeType.Length > 0;
    }

    public static string Build(string mimeType, byte[] bytes) =>
        $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
}
