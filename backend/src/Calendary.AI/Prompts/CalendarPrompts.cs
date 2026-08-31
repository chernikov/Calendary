namespace Calendary.AI.Prompts;

/// Wraps the DB-stored prompt library texts (scene descriptor + visual style descriptor) with
/// the standard before/after instructions sent to the AI image provider. Kept separate from the
/// client implementations so the wording can be tuned without touching provider integration code.
///
/// English prompts are used deliberately — both OpenAI's and Gemini's image models follow
/// English instructions more reliably than Ukrainian ones, even though the product's own copy
/// (theme/prompt/style names) is Ukrainian.
public static class CalendarPrompts
{
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

    /// <param name="sceneText">Prompt.Text — the English scene descriptor chosen for the sheet.</param>
    /// <param name="styleText">ImageStyle.Text — the English visual style descriptor chosen for the sheet.</param>
    public static string BuildCoverPrompt(string sceneText, string styleText)
    {
        return
            $"Reimagine the person in the reference photo as {sceneText}. " +
            "Keep their face clearly recognizable — same facial structure and features, just " +
            $"restyled into the scene. Render the image in this visual style: {styleText}. " +
            "Vertical 3:4 portrait composition suitable for a calendar cover. Do not add any " +
            "text, numbers, calendar grid, or watermark to the image.";
    }

    /// <param name="sceneText">Prompt.Text — the English scene descriptor chosen for the sheet.</param>
    /// <param name="styleText">ImageStyle.Text — the English visual style descriptor chosen for the sheet.</param>
    /// <param name="month">1–12.</param>
    public static string BuildMonthPrompt(string sceneText, string styleText, int month)
    {
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));

        var monthName = MonthNamesUk[month - 1];
        var season = SeasonHints[month - 1];
        return
            $"Reimagine the person in the reference photo as {sceneText}. Set the scene for " +
            $"{monthName} ({season}) in Ukraine. Keep their face clearly recognizable — same " +
            "facial structure and features, just restyled into the scene. Render the image in " +
            $"this visual style: {styleText}. Vertical 3:4 portrait composition suitable for a " +
            "calendar page. Do not add any text, numbers, calendar grid, or watermark to the image.";
    }
}
