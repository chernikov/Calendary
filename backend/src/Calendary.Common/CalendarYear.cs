namespace Calendary.Common;

/// The year every order's calendar prints for — always "next year" relative to today, computed
/// fresh rather than stored anywhere, so it advances automatically on Jan 1. Shared so personal-
/// date validation (needs the right leap-year day count for the actual printed year, see #320)
/// and PDF rendering (CalendarPdfService) never drift apart. Keep in sync with the frontend's own
/// copy of this calculation (style-dates.component.ts's `calendarYear`).
public static class CalendarYear
{
    public static int Current => DateTime.UtcNow.Year + 1;
}
