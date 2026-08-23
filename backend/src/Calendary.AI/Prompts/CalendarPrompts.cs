namespace Calendary.AI.Prompts;

/// Builds the text prompts sent to the AI image provider. Kept separate from the client
/// implementations so the wording can be tuned without touching provider integration code.
///
/// English prompts are used deliberately — both OpenAI's and Gemini's image models follow
/// English instructions more reliably than Ukrainian ones, even though the product's own copy
/// (style names, category descriptions) is Ukrainian.
public static class CalendarPrompts
{
    /// Keyed by StyleCategory.Code (see AppDbContext seed data).
    private static readonly Dictionary<string, string> StyleDescriptors = new()
    {
        ["history"] = "a legendary figure from history — choose one of: Viking warrior, Egyptian " +
                      "pharaoh, samurai, medieval knight, or Cossack — with authentic period " +
                      "clothing, armor, and props",
        ["cinema"] = "a character from classic cinema — choose one of: film noir detective, " +
                     "spaghetti western gunslinger, spy, or musical performer — styled after that " +
                     "genre's iconic look and setting",
        ["adventure"] = "an adventurer — choose one of: mountaineer, bush pilot, scuba diver, or " +
                         "polar explorer — with the gear and dramatic environment of that pursuit",
        ["professions"] = "a professional captured in action — choose one of: head chef, doctor, " +
                           "orchestra conductor, or firefighter — with the tools and setting of " +
                           "that profession"
    };

    private static readonly string[] MonthNamesUk =
    [
        "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень",
        "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень"
    ];

    /// Brief seasonal flavor for Ukraine's climate, used so the twelve monthly sheets don't all
    /// look interchangeable.
    private static readonly string[] SeasonHints =
    [
        "deep winter, snow, cold light", // January
        "late winter, snow still on the ground", // February
        "early spring, melting snow, bare trees budding", // March
        "spring in bloom, fresh green", // April
        "late spring, warm and lush", // May
        "early summer, bright daylight", // June
        "high summer, warm golden light", // July
        "late summer, sun-baked, harvest starting", // August
        "early autumn, first turning leaves", // September
        "mid-autumn, golden and red foliage", // October
        "late autumn, bare trees, overcast", // November
        "winter holidays, snow, festive evening light" // December
    ];

    /// <param name="styleCode">StyleCategory.Code — "history" | "cinema" | "adventure" | "professions".</param>
    public static string BuildCoverPrompt(string styleCode)
    {
        var descriptor = Describe(styleCode);
        return
            $"Reimagine the person in the reference photo as {descriptor}. " +
            "Keep their face clearly recognizable — same facial structure and features, just " +
            "restyled into the scene. Cinematic lighting, rich detail, vertical 3:4 portrait " +
            "composition suitable for a calendar cover. Do not add any text, numbers, calendar " +
            "grid, or watermark to the image.";
    }

    /// <param name="styleCode">StyleCategory.Code — "history" | "cinema" | "adventure" | "professions".</param>
    /// <param name="month">1–12.</param>
    public static string BuildMonthPrompt(string styleCode, int month)
    {
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));

        var descriptor = Describe(styleCode);
        var monthName = MonthNamesUk[month - 1];
        var season = SeasonHints[month - 1];
        return
            $"Reimagine the person in the reference photo as {descriptor}, keeping the same " +
            "character identity, outfit, and art style as the calendar's cover image. Set the " +
            $"scene for {monthName} ({season}) in Ukraine. Keep their face clearly recognizable. " +
            "Cinematic lighting, rich detail, vertical 3:4 portrait composition suitable for a " +
            "calendar page. Do not add any text, numbers, calendar grid, or watermark to the image.";
    }

    private static string Describe(string styleCode) =>
        StyleDescriptors.TryGetValue(styleCode, out var descriptor)
            ? descriptor
            : throw new ArgumentException($"Unknown style code '{styleCode}'.", nameof(styleCode));
}
