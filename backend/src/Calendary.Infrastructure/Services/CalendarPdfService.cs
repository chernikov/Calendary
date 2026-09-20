using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Calendary.Infrastructure.Services;

/// Renders the finished calendar (cover + 12 month sheets) as a print-style PDF: one full-bleed
/// page for the cover, then one page per month with the AI-generated image plus a rendered
/// day-grid highlighting that month's personal dates and public holidays (see #364). Caller
/// (OrdersController) is responsible for checking all 13 sheets are SheetStatus.Ready before
/// calling — see GenerateAsync's guard for why that check is repeated here too.
public class CalendarPdfService(HttpClient httpClient, AppDbContext db, IFileStorage fileStorage) : ICalendarPdfService
{
    private static readonly string[] WeekdaysMonFirst = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд"];
    private static readonly string[] WeekdaysSunFirst = ["Нд", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"];

    // Keep in sync with frontend/src/app/pages/month/month.component.ts's `MONTH_NAMES`.
    private static readonly string[] MonthNames =
    [
        "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень",
        "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень",
    ];

    // Personal dates and holidays are text-color-only now (no background fill, see #364) — blue
    // for the customer's own dates, red for state holidays/weekends, same as
    // frontend/src/styles.css would use if it ever needed to render this (it doesn't; this grid
    // only exists in the PDF).
    private static readonly Color PersonalDateColor = Color.FromHex("#0088b0");
    private static readonly Color HolidayColor = Color.FromHex("#c0392b");

    static CalendarPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAsync(Guid orderId, bool watermark, CancellationToken ct = default)
    {
        var order = await db.Orders
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets)
            // See OrdersController.LoadOwnedOrderAsync — avoids a cartesian join between the two
            // sibling collections.
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");

        if (order.Sheets.Count != 13 || order.Sheets.Any(s => s.Status != SheetStatus.Ready))
        {
            throw new InvalidOperationException($"Order {orderId} is not fully generated yet.");
        }

        var cover = order.Sheets.First(s => s.Kind == SheetKind.Cover);
        var monthSheets = order.Sheets
            .Where(s => s.Kind == SheetKind.Month)
            .OrderBy(s => s.Index)
            .ToList();

        var coverBytes = await ResolveImageBytesAsync(cover.ImageUrl, ct);
        var monthBytes = await Task.WhenAll(monthSheets.Select(s => ResolveImageBytesAsync(s.ImageUrl, ct)));

        var calendarYear = DateTime.UtcNow.Year + 1;

        // Holiday data currently only covers 2027 (see #364) — an order for a future calendarYear
        // simply gets an empty list here, degrading gracefully to "just weekends marked".
        var countries = order.HolidayCountries;
        var holidays = await db.Holidays
            .Where(h => h.Year == calendarYear && countries.Contains(h.Country))
            .ToListAsync(ct);

        var document = Document.Create(container =>
        {
            container.Page(page => ComposeCoverPage(page, coverBytes, watermark));

            for (var i = 0; i < monthSheets.Count; i++)
            {
                var month = monthSheets[i].Index;
                var imageBytes = monthBytes[i];
                var datesForMonth = order.PersonalDates.Where(d => d.Month == month).ToList();
                var holidaysForMonth = holidays.Where(h => h.Month == month).ToList();
                container.Page(page => ComposeMonthPage(
                    page, imageBytes, month, calendarYear, datesForMonth, holidaysForMonth, order.WeekStart, watermark));
            }
        });

        return document.GeneratePdf();
    }

    private async Task<byte[]> ResolveImageBytesAsync(string? imageUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new InvalidOperationException("Ready sheet has no image URL.");
        }

        if (fileStorage.IsStoredUrl(imageUrl))
        {
            return (await fileStorage.ReadAsync(imageUrl, ct)).Content;
        }

        // The mock generator still points sheets at external placeholder images.
        return await httpClient.GetByteArrayAsync(imageUrl, ct);
    }

    private static void ComposeCoverPage(PageDescriptor page, byte[] coverBytes, bool watermark)
    {
        page.Size(PageSizes.A4);
        page.Margin(24);
        page.Content().AlignCenter().AlignMiddle().Image(coverBytes).FitArea();
        if (watermark)
        {
            page.Foreground().Element(ComposeWatermark);
        }
    }

    private static void ComposeMonthPage(
        PageDescriptor page, byte[] imageBytes, int month, int calendarYear,
        IReadOnlyList<PersonalDate> dates, IReadOnlyList<Holiday> holidaysForMonth,
        WeekStartDay weekStart, bool watermark)
    {
        page.Size(PageSizes.A4);
        page.Margin(18);
        page.Content().Column(column =>
        {
            column.Spacing(8);
            column.Item().AlignCenter().Text(MonthNames[month - 1]).FontSize(20).Bold();
            // Another +20% on top of the previous 456pt (which itself was +20% over the original
            // 380pt, see #364). Centered instead of stretching/left-aligning within the column.
            column.Item().AlignCenter().Height(547).Image(imageBytes).FitArea();
            // A bit more breathing room here specifically, on top of the column's own spacing.
            column.Item().PaddingTop(6).Element(e => ComposeCalendarGrid(e, month, calendarYear, dates, holidaysForMonth, weekStart));
        });
        if (watermark)
        {
            page.Foreground().Element(ComposeWatermark);
        }
    }

    // A visible-but-unobtrusive diagonal stamp so a pre-payment PDF can't pass as the final print
    // file; DownloadPdf only omits this once the order is Paid.
    private static void ComposeWatermark(IContainer container)
    {
        container
            .AlignCenter()
            .AlignMiddle()
            .Rotate(-30)
            .Text("ПЕРЕГЛЯД — ДО ОПЛАТИ")
            .FontSize(36)
            .Bold()
            .FontColor(Color.FromHex("#80201e1d"));
    }

    // Personal-dates picker in style-dates.component.ts is a separate input tool (always
    // Monday-first, its own styling) and intentionally no longer kept in lockstep with this method
    // — this is the printed artifact; that's just how the customer enters dates (see #364).
    private static void ComposeCalendarGrid(
        IContainer container, int month, int calendarYear, IReadOnlyList<PersonalDate> dates,
        IReadOnlyList<Holiday> holidaysForMonth, WeekStartDay weekStart)
    {
        var weekdays = weekStart == WeekStartDay.Sunday ? WeekdaysSunFirst : WeekdaysMonFirst;
        var dow = (int)new DateTime(calendarYear, month, 1).DayOfWeek; // Sunday=0..Saturday=6
        var firstWeekday = weekStart == WeekStartDay.Sunday ? dow : (dow + 6) % 7;
        var daysInMonth = DateTime.DaysInMonth(calendarYear, month);

        var cells = new List<int?>();
        cells.AddRange(Enumerable.Repeat((int?)null, firstWeekday));
        cells.AddRange(Enumerable.Range(1, daysInMonth).Select(d => (int?)d));
        // Pad to a whole number of weeks (5 or 6 rows depending on the month/week-start), not
        // always a fixed 6 — every extra row costs real vertical space now that cells are taller
        // and the page also carries a bigger image plus a holiday legend (see #364).
        while (cells.Count % 7 != 0)
        {
            cells.Add(null);
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (var i = 0; i < 7; i++)
                {
                    columns.RelativeColumn();
                }
            });

            foreach (var weekday in weekdays)
            {
                table.Cell().Padding(2).AlignCenter().Text(weekday).FontSize(9).FontColor(Colors.Grey.Darken1);
            }

            foreach (var cell in cells)
            {
                if (cell is null)
                {
                    table.Cell().Border(1).BorderColor(Colors.Black).MinHeight(32);
                    continue;
                }

                var day = cell.Value;
                var dayDates = dates.Where(d => d.Day == day).ToList();
                var isPersonal = dayDates.Count > 0;
                var actualDayOfWeek = new DateTime(calendarYear, month, day).DayOfWeek;
                var isWeekend = actualDayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                var holidayForDay = isPersonal ? null : holidaysForMonth.FirstOrDefault(h => h.Day == day);
                var textColor = isPersonal ? PersonalDateColor : isWeekend || holidayForDay is not null ? HolidayColor : Colors.Black;

                // No padding/margin between cells — borders sit flush against each other, forming
                // one continuous grid instead of a table of separated boxes (see #374).
                table.Cell().Element(cellContainer =>
                {
                    cellContainer
                        .MinHeight(32)
                        .Border(1)
                        .BorderColor(Colors.Black)
                        .Padding(2)
                        .Column(dayColumn =>
                        {
                            dayColumn.Item().AlignLeft().Text(day.ToString()).FontSize(9).FontColor(textColor);
                            if (isPersonal)
                            {
                                dayColumn.Item().AlignCenter().Text(string.Join(", ", dayDates.Select(d => d.Label)))
                                    .FontSize(5.5f)
                                    .FontColor(PersonalDateColor);
                            }
                            else if (holidayForDay is not null)
                            {
                                // Curated short label (admin-managed, see #376) — left-aligned so it
                                // reads as continuing after the date, and can wrap onto a second
                                // line within the cell instead of being mechanically truncated.
                                dayColumn.Item().AlignLeft().Text(holidayForDay.ShortName)
                                    .FontSize(5.5f)
                                    .FontColor(HolidayColor);
                            }
                        });
                });
            }
        });
    }
}
