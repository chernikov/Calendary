using System;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.IO.Font;
using iText.Kernel.Font;

class Program
{
    static void Main()
    {
        // Define the path for the PDF file
        string dest = "a3_calendar_2025.pdf";

        // Create a PDF writer
        PdfWriter writer = new PdfWriter(dest);

        // Initialize a PDF document with A3 size
        PdfDocument pdf = new PdfDocument(writer);
        pdf.SetDefaultPageSize(PageSize.A3);

        // Initialize a document
        Document document = new Document(pdf);

        // Load the font from the specified path
        PdfFont font = PdfFontFactory.CreateFont("fonts/arial.ttf", PdfEncodings.IDENTITY_H);

        // Month names and corresponding image paths
        string[] months = { "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень" };
        string[] imagePaths = {
            "content/january.jpg", "content/february.jpg", "content/march.jpg", "content/april.jpg",
            "content/may.jpg", "content/june.jpg", "content/july.jpg", "content/august.jpg",
            "content/september.jpg", "content/october.jpg", "content/november.jpg", "content/december.jpg"
        };

        // Days in each month for 2025
        int[][] daysInMonth = {
            new int[] { 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 },
            new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 },
            new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 },
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 },
            new int[] { 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 },
            new int[] { 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 },
            new int[] { 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 },
            new int[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 },
            new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 },
            new int[] { 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 },
            new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 },
            new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }
        };

        // Ukrainian holidays and special days in 2025 (day, month, description)
        var specialDays = new (int, int, string)[]
        {
            (1, 1, "Новий рік"),
            (8, 5, "День пам'яті"),
            (1, 5, "День праці"),
            (28, 6, "День Конституції"),
            (24, 8, "День Незалежності"),
            (1, 10, "День захисника"),
            (25, 12, "Різдво"),
            (25, 5, "11 років весілля"),
            (15, 6, "5 років Максиму"),
            (3, 7, "10 років Жанні"),
            (12, 9, "42 роки Андрію"),
            (14, 12, "44 роки Анні")
        };

        for (int i = 0; i < 12; i++)
        {
            // Add a new page for each month
            document.Add(new AreaBreak());

            // Load the image for the current month
            ImageData imageData = ImageDataFactory.Create(imagePaths[i]);
            Image image = new Image(imageData);

            // Calculate the scaling factor to fit the image to half the page while maintaining aspect ratio
            float pageWidth = pdf.GetDefaultPageSize().GetWidth();
            float pageHeight = pdf.GetDefaultPageSize().GetHeight() / 2;
            float imageWidth = image.GetImageWidth();
            float imageHeight = image.GetImageHeight();
            float scaleFactor = Math.Min(pageWidth / imageWidth, pageHeight / imageHeight) * 1.2f; // Increase size by 20%

            // Apply the scaling factor
            image.Scale(scaleFactor, scaleFactor);

            // Center the image horizontally
            image.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            // Add the image to the document
            document.Add(image);

            // Add a title
            Paragraph title = new Paragraph(months[i])
                .SetFont(font)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(24);
            document.Add(title);

            // Create a table for the calendar
            Table table = new Table(UnitValue.CreatePercentArray(7)).UseAllAvailableWidth();

            // Add day headers in Ukrainian, starting with Monday
            string[] days = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };
            foreach (var day in days)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(day))
                    .SetFont(font)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            }

            // Fill the calendar with days for the current month
            for (int j = 0; j < daysInMonth[i].Length; j++)
            {
                int day = daysInMonth[i][j];
                Color fontColor = ColorConstants.GRAY;
                string description = "";

                // Check if the day is a weekend (Saturday or Sunday)
                if (j % 7 == 5 || j % 7 == 6)
                {
                    fontColor = ColorConstants.RED;
                }

                // Check if the day is a special day
                foreach (var specialDay in specialDays)
                {
                    if (specialDay.Item1 == day && specialDay.Item2 == i + 1)
                    {
                        fontColor = ColorConstants.RED;
                        description = specialDay.Item3;
                        break;
                    }
                }

                Cell cell = new Cell().Add(new Paragraph(day == 0 ? "" : $"{day}\n{description}")
                    .SetFont(font)
                    .SetFontSize(10) // Smaller font size
                    .SetFontColor(fontColor)) // Set color
                    .SetTextAlignment(TextAlignment.LEFT) // Align to top left
                    .SetVerticalAlignment(VerticalAlignment.TOP)
                    .SetHeight(50); // Set height to make cells square
                table.AddCell(cell);
            }

            // Add the table to the document
            document.Add(table);
        }

        // Close the document
        document.Close();

        Console.WriteLine("A3 PDF with 2025 calendar created.");
    }
}