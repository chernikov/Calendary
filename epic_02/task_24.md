# Task 24: PDF Generation для календарів

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 8-10 годин
**Відповідальний AI**: Claude
**Паралельно з**: Task 21, 22, 23

## Опис задачі

Імплементувати PDF generation для календарів використовуючи QuestPDF, підтримка різних форматів (A3, A4), 300 DPI якість для друку.

## Проблема

Користувачі повинні мати можливість отримати високоякісний PDF файл календаря для друку (самостійно або через друкарню).

## Що треба зробити

1. **Інсталювати QuestPDF**
   ```bash
   dotnet add package QuestPDF
   ```
   - License: Community (free для non-commercial)

2. **Створити PDF Template Service**
   - `src/Calendary.Application/Services/PdfGenerationService.cs`
   - Генерація PDF з canvas design data
   - Підтримка форматів A3, A4
   - 300 DPI якість

3. **Calendar PDF Template**
   - Layout:
     - Header з місяцем/роком
     - Grid календаря (7x5 або 7x6)
     - User images з canvas
     - Text elements з canvas
   - Fonts: підтримка кириличних шрифтів

4. **Image Processing**
   - Отримати images з canvas design data
   - Download images з URLs
   - Resize для PDF (high quality)
   - Embed в PDF

5. **Background Processing**
   - PDF generation може займати час
   - Використовувати background jobs (Hangfire або RabbitMQ)
   - Notification коли PDF готовий

6. **API Endpoints**
   - POST /api/calendars/{id}/generate-pdf - створити PDF
   - GET /api/calendars/{id}/pdf-status - статус генерації
   - GET /api/calendars/{id}/download-pdf - завантажити PDF

7. **Storage**
   - Зберігати PDF в blob storage
   - Тимчасові файли (1 день)
   - Або permanent після оплати

## Файли для створення/модифікації

- `src/Calendary.Core/Interfaces/IPdfGenerationService.cs`
- `src/Calendary.Application/Services/PdfGenerationService.cs`
- `src/Calendary.Application/Templates/CalendarPdfTemplate.cs`
- `src/Calendary.API/Controllers/CalendarsController.cs` - PDF endpoints
- `src/Calendary.Infrastructure/Jobs/GeneratePdfJob.cs` (background)

## Критерії успіху

- [ ] PDF генерується з canvas design data
- [ ] Підтримуються формати A3 та A4
- [ ] Якість 300 DPI для друку
- [ ] Images з canvas embedded в PDF
- [ ] Text з canvas rendered правильно
- [ ] Кириличні шрифти працюють
- [ ] PDF можна завантажити через API
- [ ] Background processing працює

## Залежності

Немає (незалежна задача)

## Блокується наступні задачі

- Task 25: Download потребує PDF generation

## Технічні деталі

### PdfGenerationService.cs
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public interface IPdfGenerationService
{
    Task<string> GeneratePdfAsync(Guid calendarId, PdfFormat format);
    Task<PdfGenerationStatus> GetStatusAsync(Guid jobId);
}

public class PdfGenerationService : IPdfGenerationService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IBackgroundJobClient _jobClient;

    public async Task<string> GeneratePdfAsync(Guid calendarId, PdfFormat format)
    {
        var calendar = await _context.UserCalendars
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == calendarId);

        if (calendar == null)
            throw new NotFoundException("Calendar not found");

        // Enqueue background job
        var jobId = _jobClient.Enqueue(() => GeneratePdfBackgroundAsync(calendarId, format));

        return jobId;
    }

    public async Task GeneratePdfBackgroundAsync(Guid calendarId, PdfFormat format)
    {
        var calendar = await _context.UserCalendars
            .FirstOrDefaultAsync(c => c.Id == calendarId);

        var designData = JsonSerializer.Deserialize<CanvasDesign>(calendar.DesignData);

        // Generate PDF
        var pdfBytes = await CreatePdfDocument(designData, format);

        // Upload to storage
        var fileName = $"calendar-{calendarId}-{DateTime.UtcNow:yyyyMMdd}.pdf";
        var url = await _fileStorage.UploadAsync(fileName, pdfBytes, "application/pdf");

        // Update calendar
        calendar.PdfUrl = url;
        calendar.PdfGeneratedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task<byte[]> CreatePdfDocument(CanvasDesign design, PdfFormat format)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Page size based on format
                if (format == PdfFormat.A3)
                {
                    page.Size(PageSizes.A3);
                }
                else
                {
                    page.Size(PageSizes.A4);
                }

                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                page.Content().Column(column =>
                {
                    // Render calendar header
                    column.Item().Text("Календар 2025")
                        .FontSize(24)
                        .Bold()
                        .AlignCenter();

                    column.Item().PaddingVertical(10);

                    // Render canvas content
                    RenderCanvasContent(column, design);

                    // Render calendar grid
                    RenderCalendarGrid(column);
                });
            });
        });

        return document.GeneratePdf();
    }

    private void RenderCanvasContent(ColumnDescriptor column, CanvasDesign design)
    {
        foreach (var obj in design.Objects)
        {
            if (obj.Type == "image")
            {
                // Download image
                var imageBytes = DownloadImage(obj.Src);

                column.Item()
                    .Width(obj.Width)
                    .Height(obj.Height)
                    .Image(imageBytes);
            }
            else if (obj.Type == "text")
            {
                column.Item()
                    .Text(obj.Text)
                    .FontSize(obj.FontSize)
                    .FontColor(obj.Fill);
            }
        }
    }

    private void RenderCalendarGrid(ColumnDescriptor column)
    {
        var daysInMonth = DateTime.DaysInMonth(2025, 1);
        var firstDay = new DateTime(2025, 1, 1).DayOfWeek;

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (int i = 0; i < 7; i++)
                    columns.RelativeColumn();
            });

            // Header (days of week)
            var daysOfWeek = new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };
            foreach (var day in daysOfWeek)
            {
                table.Cell().Border(1).Padding(5).Text(day).Bold();
            }

            // Calendar days
            int currentDay = 1;
            for (int week = 0; week < 6; week++)
            {
                for (int day = 0; day < 7; day++)
                {
                    if ((week == 0 && day < (int)firstDay) || currentDay > daysInMonth)
                    {
                        table.Cell().Border(1).Padding(5);
                    }
                    else
                    {
                        table.Cell().Border(1).Padding(5).Text(currentDay.ToString());
                        currentDay++;
                    }
                }
            }
        });
    }

    private byte[] DownloadImage(string url)
    {
        using var client = new HttpClient();
        return client.GetByteArrayAsync(url).Result;
    }
}
```

### CanvasDesign Model
```csharp
public class CanvasDesign
{
    public List<CanvasObject> Objects { get; set; }
}

public class CanvasObject
{
    public string Type { get; set; } // "image", "text", "shape"
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    // Image properties
    public string Src { get; set; }

    // Text properties
    public string Text { get; set; }
    public double FontSize { get; set; }
    public string FontFamily { get; set; }
    public string Fill { get; set; } // Color
}
```

## Примітки

- QuestPDF - сучасна бібліотека для .NET
- 300 DPI важливо для друку (не менше)
- Background jobs запобігають timeout
- Кириличні шрифти потрібно embedded

## Чому Claude?

Складна технічна задача:
- PDF generation library integration
- Canvas to PDF conversion logic
- Image processing та embedding
- Background jobs
- Performance optimization

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
