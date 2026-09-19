namespace Calendary.AI.Clients;

internal static class DataUrl
{
    /// Splits a "data:image/png;base64,AAAA..." string into its MIME type and raw bytes.
    public static (string MimeType, byte[] Bytes) Parse(string dataUrl)
    {
        var comma = dataUrl.IndexOf(',');
        if (!dataUrl.StartsWith("data:", StringComparison.Ordinal) || comma < 0)
        {
            throw new FormatException("Expected a data: URL (e.g. data:image/png;base64,...).");
        }

        var header = dataUrl[5..comma]; // "image/png;base64"
        var mimeType = header.Split(';')[0];
        var bytes = Convert.FromBase64String(dataUrl[(comma + 1)..]);
        return (mimeType, bytes);
    }

    public static string Build(string mimeType, byte[] bytes) =>
        $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";

    public static string Build(string mimeType, string base64) =>
        $"data:{mimeType};base64,{base64}";
}
