using Calendary.Repos.Repositories;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;
using ImagePdf = iText.Layout.Element.Image;
using CalendarModel = Calendary.Model.Calendar;
using Calendary.Core.Providers;
using iText.Layout.Borders;

namespace Calendary.Core.Services;

public interface IPdfGeneratorService
{
    Task<string> GeneratePdfAsync(int calendarId);
}

public class PdfGeneratorService(ICalendarRepository calendarRepository,
    IPathProvider pathProvider,
    IImageRotatorService imageRotatorService
    ) : IPdfGeneratorService
{
    private PdfFont Font { get; init; } = PdfFontFactory.CreateFont("fonts/arial.ttf", PdfEncodings.IDENTITY_H);
    public async Task<string> GeneratePdfAsync(int calendarId)
    {
        var calendar = await calendarRepository.GetFullCalendarAsync(calendarId);
        if (calendar is null)
        {
            return "";
        }
        var images = calendar.Images.ToArray();

        var result = GenerateCalendarPdf(calendar, images);
        return result;
    }


    public string GenerateCalendarPdf(CalendarModel calendar, Model.Image[] images)
    {
        // Визначаємо шлях до PDF
        var savedPath = System.IO.Path.Combine("uploads", $"calendar_{calendar.Id}_{calendar.Year}.pdf");
        string dest = pathProvider.MapPath(savedPath);

        // Імена місяців
        string[] months = GetMonthNames(calendar.Language.Code);
        string[] days = GetDayNames(calendar.Language.Code, calendar.FirstDayOfWeek);

        // Створюємо PDF writer
        using (PdfWriter writer = new PdfWriter(dest))
        {
            // Ініціалізуємо PDF документ з розміром A3
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                pdf.SetDefaultPageSize(PageSize.A3);
                using (Document document = new Document(pdf))
                {
                    // Додаємо по сторінці для кожного місяця
                    for (int i = 0; i < months.Length; i++)
                    {
                        AddImageOnPage(images[i], pdf, document);
                        AddHeaderOfPage(calendar.Language.Code, months[i], document);

                        var table = CreateTable(calendar, days, i);
                        document.Add(table);
                        if (i + 1 < months.Length)
                        {
                            document.Add(new AreaBreak());
                        }
                    }
                }
            }
        }
        return savedPath;
    }

    private Table CreateTable(CalendarModel calendar, string[] days, int monthIndex)
    {
        var holidaysDtos = calendar.CalendarHolidays.Select(p => new HolidayDto()
        {
            Description = p.Holiday.Name,
            Day = p.Holiday.Date.Day,
            Month = p.Holiday.Date.Month
        }).ToArray();

        var eventDateDtos = calendar.EventDates.Select(p => new EventDateDto()
        {
            Description = p.Description,
            Day = p.Date.Day,
            Month = p.Date.Month
        }).ToArray();

        // Створюємо таблицю з 7 стовпцями (дні тижня)
        Table table = new Table(UnitValue.CreatePercentArray(7)).UseAllAvailableWidth();

        // Колір рамки
        var borderColor = new DeviceRgb(211, 211, 211); // Світло-сірий (LightGray)

        // Внутрішній відступ для клітинок
        float cellPadding = 4f;

        // Додаємо заголовки для днів тижня
        foreach (var day in days)
        {
            table.AddHeaderCell(new Cell().Add(new Paragraph(day))
                .SetFont(Font)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(null)
                .SetPadding(cellPadding)); // Відступи всередині заголовку
        }

        // Визначаємо кількість днів у поточному місяці
        int daysInMonth = DateTime.DaysInMonth(calendar.Year, monthIndex + 1);

        // Визначаємо, з якого дня тижня починається місяць
        DateTime firstDayOfMonth = new DateTime(calendar.Year, monthIndex + 1, 1);
        int startDayOfWeek = ((int)firstDayOfMonth.DayOfWeek - (int)calendar.FirstDayOfWeek + 7) % 7;

        // Додаємо порожні клітинки перед першим днем місяця
        for (int i = 0; i < startDayOfWeek; i++)
        {
            table.AddCell(new Cell().Add(new Paragraph(""))
                .SetBorder(new SolidBorder(borderColor, 1))); // Відступи для порожніх клітинок
        }

        // Додаємо дні місяця в таблицю
        for (int day = 1; day <= daysInMonth; day++)
        {
            var currentDate = new DateTime(calendar.Year, monthIndex + 1, day);

            // Визначаємо колір шрифту для дати
            var fontColor = GetFontColorForDay(currentDate, holidaysDtos, eventDateDtos);

            // Отримуємо опис події або свята
            string description = GetEventDescription(day, monthIndex + 1, holidaysDtos, eventDateDtos);

            // Створюємо клітинку
            Cell cell = new Cell()
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP) // Вирівнювання по центру вертикалі
                .SetHeight(60)
                .SetBorder(new SolidBorder(borderColor, 1))
                .SetPadding(cellPadding);

            // Додаємо дату і опис в один рядок
            Paragraph content = new Paragraph()
                .Add(new Text($"{day} ").SetFont(Font).SetFontSize(18).SetFontColor(fontColor)) // Число
                .Add(new Text(description).SetFont(Font).SetFontSize(10).SetFontColor(fontColor)); // Опис

            cell.Add(content);
            table.AddCell(cell);
        }

        // Заповнюємо залишок рядка порожніми клітинками
        int remainingCells = (7 - (startDayOfWeek + daysInMonth) % 7) % 7;
        for (int i = 0; i < remainingCells; i++)
        {
            table.AddCell(new Cell().Add(new Paragraph(""))
                .SetHeight(60)
                .SetBorder(new SolidBorder(borderColor, 1))
                .SetPadding(cellPadding)); // Відступи для порожніх клітинок
        }

        return table;
    }

    private Color GetFontColorForDay(DateTime date, HolidayDto[] holidays, EventDateDto[] eventDates)
    {
        // Перевіряємо, чи є дата святом або важливою подією
        bool isHoliday = holidays.Any(h => h.Day == date.Day && h.Month == date.Month);
        bool isEventDate = eventDates.Any(e => e.Day == date.Day && e.Month == date.Month);

        // Якщо це вихідний, свято або подія, повертаємо червоний колір
        if (isHoliday || isEventDate || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            return ColorConstants.RED;
        }

        // Інакше повертаємо чорний колір
        return ColorConstants.BLACK;
    }

    private string GetEventDescription(int day, int month, HolidayDto[] holidays, EventDateDto[] eventDates)
    {
        var holiday = holidays.FirstOrDefault(h => h.Day == day && h.Month == month);
        if (holiday != null) return holiday.Description;

        var eventDate = eventDates.FirstOrDefault(e => e.Day == day && e.Month == month);
        if (eventDate != null) return eventDate.Description;

        return "";
    }

    private void AddImageOnPage(Model.Image image, PdfDocument pdf, Document document)
    {
        // Завантажуємо зображення для місяця
        var pathImage = pathProvider.MapPath(image.ImageUrl);
        var imagePdf = imageRotatorService.LoadCorrectedImage(pathImage);
        ScaleImage(pdf, imagePdf);

        document.Add(imagePdf);
    }

    private static void ScaleImage(PdfDocument pdf, ImagePdf imagePdf)
    {
        // Масштабуємо зображення
        float pageWidth = pdf.GetDefaultPageSize().GetWidth();
        float pageHeight = pdf.GetDefaultPageSize().GetHeight() / 2f; // Зменшено висоту зображення
        float scaleFactor = Math.Min(pageWidth / imagePdf.GetImageWidth(), pageHeight / imagePdf.GetImageHeight()) * 1.0f; // Трохи зменшено коефіцієнт
        imagePdf.Scale(scaleFactor, scaleFactor).SetHorizontalAlignment(HorizontalAlignment.CENTER);
    }

    private void AddHeaderOfPage(string langCode, string month, Document document)
    {
        // Capitalize the first letter of the month
        string capitalizedMonth = new CultureInfo("uk-UA").TextInfo.ToTitleCase(month);

        // Add the capitalized month as the header
        Paragraph title = new Paragraph(capitalizedMonth)
            .SetFont(Font)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(30);
        document.Add(title);
    }


    private static string[] GetMonthNames(string languageCode)
    {
        var culture = new CultureInfo(languageCode);
        return culture.DateTimeFormat.MonthNames[..12]; // Отримуємо 12 назв місяців
    }

    private static string[] GetDayNames(string languageCode, DayOfWeek firstDayOfWeek)
    {
        var culture = new CultureInfo(languageCode);
        var dayNames = culture.DateTimeFormat.AbbreviatedDayNames;

        // Створюємо новий масив зі зміщенням відповідно до FirstDayOfWeek
        string[] reorderedDayNames = new string[7];
        int startIndex = (int)firstDayOfWeek; // Початкова позиція у масиві

        for (int i = 0; i < 7; i++)
        {
            reorderedDayNames[i] = dayNames[(startIndex + i) % 7];
        }

        return reorderedDayNames;
    }

    private class EventDateDto
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Description { get; set; } = null!;
    }

    private class HolidayDto
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Description { get; set; } = null!;
    }
}
