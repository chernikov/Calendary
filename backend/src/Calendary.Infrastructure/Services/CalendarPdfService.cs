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
/// day-grid highlighting that month's personal dates. Caller (OrdersController) is responsible
/// for checking all 13 sheets are SheetStatus.Ready before calling — see GenerateAsync's guard
/// for why that check is repeated here too.
public class CalendarPdfService(HttpClient httpClient, AppDbContext db, IFileStorage fileStorage) : ICalendarPdfService
{
    // Keep in sync with frontend/src/app/pages/style-dates/style-dates.component.ts's `weekdays`.
    private static readonly string[] Weekdays = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд"];

    // Keep in sync with frontend/src/app/pages/month/month.component.ts's `MONTH_NAMES`.
    private static readonly string[] MonthNames =
    [
        "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень",
        "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень",
    ];

    // Same teal-blue accent as frontend/src/styles.css's --color-accent / --color-accent-100,
    // used there for .calendar-day.has-date — kept visually consistent with the web UI.
    private static readonly Color AccentColor = Color.FromHex("#0088b0");
    private static readonly Color AccentBackground = Color.FromHex("#e3f3f7");

    static CalendarPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAsync(Guid orderId, CancellationToken ct = default)
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

        var document = Document.Create(container =>
        {
            container.Page(page => ComposeCoverPage(page, coverBytes));

            for (var i = 0; i < monthSheets.Count; i++)
            {
                var month = monthSheets[i].Index;
                var imageBytes = monthBytes[i];
                var datesForMonth = order.PersonalDates.Where(d => d.Month == month).ToList();
                container.Page(page => ComposeMonthPage(page, imageBytes, month, calendarYear, datesForMonth));
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

    private static void ComposeCoverPage(PageDescriptor page, byte[] coverBytes)
    {
        page.Size(PageSizes.A4);
        page.Margin(0);
        page.Content().Image(coverBytes).FitArea();
    }

    private static void ComposeMonthPage(
        PageDescriptor page, byte[] imageBytes, int month, int calendarYear, IReadOnlyList<PersonalDate> dates)
    {
        page.Size(PageSizes.A4);
        page.Margin(24);
        page.Content().Column(column =>
        {
            column.Spacing(12);
            column.Item().Text(MonthNames[month - 1]).FontSize(20).Bold();
            column.Item().Height(380).Image(imageBytes).FitArea();
            column.Item().Element(e => ComposeCalendarGrid(e, month, calendarYear, dates));
        });
    }

    // Direct port of calendarCells()/hasDate() from
    // frontend/src/app/pages/style-dates/style-dates.component.ts — keep both in sync.
    private static void ComposeCalendarGrid(
        IContainer container, int month, int calendarYear, IReadOnlyList<PersonalDate> dates)
    {
        var firstWeekday = ((int)new DateTime(calendarYear, month, 1).DayOfWeek + 6) % 7;
        var daysInMonth = DateTime.DaysInMonth(calendarYear, month);

        var cells = new List<int?>();
        cells.AddRange(Enumerable.Repeat((int?)null, firstWeekday));
        cells.AddRange(Enumerable.Range(1, daysInMonth).Select(d => (int?)d));
        while (cells.Count < 42)
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

            foreach (var weekday in Weekdays)
            {
                table.Cell().Padding(2).AlignCenter().Text(weekday).FontSize(9).FontColor(Colors.Grey.Darken1);
            }

            foreach (var cell in cells)
            {
                if (cell is null)
                {
                    table.Cell().Padding(2).MinHeight(28);
                    continue;
                }

                var day = cell.Value;
                var dayDates = dates.Where(d => d.Day == day).ToList();
                var isHighlighted = dayDates.Count > 0;

                table.Cell().Padding(1).Element(cellContainer =>
                {
                    var styled = cellContainer.MinHeight(28).Padding(2);
                    styled = isHighlighted
                        ? styled.Background(AccentBackground).Border(1).BorderColor(AccentColor)
                        : styled;

                    styled.Column(dayColumn =>
                    {
                        dayColumn.Item().AlignCenter().Text(day.ToString())
                            .FontSize(9)
                            .FontColor(isHighlighted ? AccentColor : Colors.Black);
                        if (isHighlighted)
                        {
                            dayColumn.Item().AlignCenter().Text(string.Join(", ", dayDates.Select(d => d.Label)))
                                .FontSize(5.5f)
                                .FontColor(AccentColor);
                        }
                    });
                });
            }
        });
    }
}
