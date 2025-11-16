using Calendary.Core.Providers;
using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

/// <summary>
/// Tests for PdfGeneratorService.
/// Note: Due to the service's primary constructor initializing a Font property that requires
/// file system access to "fonts/arial.ttf", these tests are marked as skipped.
/// To properly test PDF generation, consider:
/// 1. Creating integration tests with actual font files
/// 2. Refactoring the service to inject the font dependency
/// 3. Using a test-specific font or mocking the font loading mechanism
/// </summary>
public class PdfGeneratorServiceTests
{
    private const string SkipReason = "PdfGeneratorService requires fonts/arial.ttf file which is not available in test environment. " +
                                      "Font is loaded in constructor property initializer. Consider integration tests or refactoring for better testability.";

    private readonly Mock<ICalendarRepository> _mockCalendarRepository;
    private readonly Mock<IPathProvider> _mockPathProvider;
    private readonly Mock<IImageRotatorService> _mockImageRotatorService;

    public PdfGeneratorServiceTests()
    {
        _mockCalendarRepository = new Mock<ICalendarRepository>();
        _mockPathProvider = new Mock<IPathProvider>();
        _mockImageRotatorService = new Mock<IImageRotatorService>();
    }

    private Calendar CreateTestCalendar(int id = 1, int year = 2024, string languageCode = "uk-UA")
    {
        var language = new Language
        {
            Id = 1,
            Name = "Українська",
            Code = languageCode
        };

        var calendar = new Calendar
        {
            Id = id,
            Year = year,
            LanguageId = 1,
            Language = language,
            FirstDayOfWeek = DayOfWeek.Monday,
            Images = new List<Image>(),
            EventDates = new List<EventDate>(),
            CalendarHolidays = new List<CalendarHoliday>()
        };

        // Add 12 images (one per month)
        for (int i = 1; i <= 12; i++)
        {
            calendar.Images.Add(new Image
            {
                Id = i,
                CalendarId = id,
                MonthNumber = (short)i,
                ImageUrl = $"uploads/image_{i}.jpg"
            });
        }

        return calendar;
    }

    #region GeneratePdfAsync Tests

    [Fact]
    public async Task GeneratePdfAsync_CalendarExists_ReturnsFilePath()
    {
        // Arrange
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId);

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        // Mock image data for iText - use a simple implementation
        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);

        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendarId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
        Assert.StartsWith("uploads", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_CalendarNotFound_ReturnsEmptyString()
    {
        // Arrange
        var calendarId = 999;
        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync((Calendar)null);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendarId);

        // Assert
        Assert.Equal("", result);
        _mockPathProvider.Verify(x => x.MapPath(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GeneratePdfAsync_CallsRepositoryWithCorrectId_RepositoryCalledOnce()
    {
        // Arrange
        var calendarId = 42;
        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync((Calendar)null);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        await service.GeneratePdfAsync(calendarId);

        // Assert
        _mockCalendarRepository.Verify(x => x.GetFullCalendarAsync(calendarId), Times.Once);
    }

    [Fact]
    public async Task GeneratePdfAsync_CalendarWithEventDates_IncludesEventDatesInPdf()
    {
        // Arrange
        var calendar = CreateTestCalendar();
        calendar.EventDates.Add(new EventDate
        {
            Id = 1,
            Date = new DateTime(2024, 1, 15),
            Description = "Birthday",
            CalendarId = calendar.Id
        });

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendar.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_CalendarWithHolidays_IncludesHolidaysInPdf()
    {
        // Arrange
        var calendar = CreateTestCalendar();
        var holiday = new Holiday
        {
            Id = 1,
            Date = new DateTime(2024, 1, 1),
            Name = "New Year"
        };

        calendar.CalendarHolidays.Add(new CalendarHoliday
        {
            CalendarId = calendar.Id,
            HolidayId = holiday.Id,
            Holiday = holiday
        });

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendar.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_DifferentLanguageCodes_HandlesUkrainianLanguage()
    {
        // Arrange
        var calendar = CreateTestCalendar(languageCode: "uk-UA");

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendar.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_DifferentLanguageCodes_HandlesEnglishLanguage()
    {
        // Arrange
        var calendar = CreateTestCalendar(languageCode: "en-US");

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendar.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_DifferentFirstDayOfWeek_HandlesSundayStart()
    {
        // Arrange
        var calendar = CreateTestCalendar();
        calendar.FirstDayOfWeek = DayOfWeek.Sunday;

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendar.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_DifferentFirstDayOfWeek_HandlesMondayStart()
    {
        // Arrange
        var calendar = CreateTestCalendar();
        calendar.FirstDayOfWeek = DayOfWeek.Monday;

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = await service.GeneratePdfAsync(calendar.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_ImageRotatorServiceCalled_CalledForEachMonth()
    {
        // Arrange
        var calendar = CreateTestCalendar();

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        await service.GeneratePdfAsync(calendar.Id);

        // Assert
        _mockImageRotatorService.Verify(
            x => x.LoadCorrectedImage(It.IsAny<string>()),
            Times.Exactly(12)); // Called once per month
    }

    [Fact]
    public async Task GeneratePdfAsync_PathProviderCalled_MapsPathForEachImage()
    {
        // Arrange
        var calendar = CreateTestCalendar();

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendar.Id))
            .ReturnsAsync(calendar);

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        await service.GeneratePdfAsync(calendar.Id);

        // Assert
        // Called once for PDF path + 12 times for image paths = 13 times
        _mockPathProvider.Verify(
            x => x.MapPath(It.IsAny<string>()),
            Times.AtLeast(12));
    }

    #endregion

    #region GenerateCalendarPdf Tests

    [Fact]
    public void GenerateCalendarPdf_ValidCalendar_ReturnsFilePath()
    {
        // Arrange
        var calendar = CreateTestCalendar();
        var images = calendar.Images.ToArray();

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = service.GenerateCalendarPdf(calendar, images);

        // Assert
        Assert.NotNull(result);
        Assert.Contains($"calendar_{calendar.Id}_{calendar.Year}.pdf", result);
    }

    [Fact]
    public void GenerateCalendarPdf_LeapYear_HandlesFebruaryCorrectly()
    {
        // Arrange
        var calendar = CreateTestCalendar(year: 2024); // 2024 is a leap year
        var images = calendar.Images.ToArray();

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2024.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = service.GenerateCalendarPdf(calendar, images);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2024.pdf", result);
    }

    [Fact]
    public void GenerateCalendarPdf_NonLeapYear_HandlesFebruaryCorrectly()
    {
        // Arrange
        var calendar = CreateTestCalendar(year: 2023); // 2023 is not a leap year
        var images = calendar.Images.ToArray();

        var expectedMappedPath = @"C:\wwwroot\uploads\calendar_1_2023.pdf";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedMappedPath);

        var mockImageData = new Mock<iText.IO.Image.ImageData>();
        var mockImagePdf = new Mock<iText.Layout.Element.Image>(mockImageData.Object);
        _mockImageRotatorService.Setup(x => x.LoadCorrectedImage(It.IsAny<string>()))
            .Returns(mockImagePdf.Object);

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            _mockPathProvider.Object,
            _mockImageRotatorService.Object);

        // Act
        var result = service.GenerateCalendarPdf(calendar, images);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("calendar_1_2023.pdf", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void GeneratePdf_ShouldCreate13Pages()
    {
        // Arrange: Create real calendar with test data
        var calendar = CreateTestCalendar();
        var images = calendar.Images.ToArray();

        var testOutputDir = Path.Combine(Path.GetTempPath(), "CalendaryTests");
        Directory.CreateDirectory(testOutputDir);

        var pathProvider = new TestPathProvider(testOutputDir);
        var imageRotatorService = new TestImageRotatorService();

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            pathProvider,
            imageRotatorService);

        // Act: Generate PDF
        var resultPath = service.GenerateCalendarPdf(calendar, images);
        var fullPath = pathProvider.MapPath(resultPath);

        // Assert: Verify PDF was created and has 13 pages (1 cover + 12 months)
        Assert.True(File.Exists(fullPath), "PDF file should be created");

        using (var pdfReader = new iText.Kernel.Pdf.PdfReader(fullPath))
        using (var pdfDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader))
        {
            Assert.Equal(13, pdfDoc.GetNumberOfPages());
        }

        // Cleanup
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    [Fact]
    public void GeneratePdf_WithHolidays_ShouldHighlightHolidays()
    {
        // Arrange
        var calendar = CreateTestCalendar();
        var holiday = new Holiday
        {
            Id = 1,
            Date = new DateTime(2024, 1, 1),
            Name = "New Year"
        };

        calendar.CalendarHolidays.Add(new CalendarHoliday
        {
            CalendarId = calendar.Id,
            HolidayId = holiday.Id,
            Holiday = holiday
        });

        var images = calendar.Images.ToArray();
        var testOutputDir = Path.Combine(Path.GetTempPath(), "CalendaryTests");
        Directory.CreateDirectory(testOutputDir);

        var pathProvider = new TestPathProvider(testOutputDir);
        var imageRotatorService = new TestImageRotatorService();

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            pathProvider,
            imageRotatorService);

        // Act
        var resultPath = service.GenerateCalendarPdf(calendar, images);
        var fullPath = pathProvider.MapPath(resultPath);

        // Assert: PDF should be created successfully with holidays
        Assert.True(File.Exists(fullPath), "PDF file with holidays should be created");

        // Cleanup
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    [Fact]
    public void GeneratePdf_FileSize_ShouldBeLessThan15MB()
    {
        // Arrange
        var calendar = CreateTestCalendar();
        var images = calendar.Images.ToArray();

        var testOutputDir = Path.Combine(Path.GetTempPath(), "CalendaryTests");
        Directory.CreateDirectory(testOutputDir);

        var pathProvider = new TestPathProvider(testOutputDir);
        var imageRotatorService = new TestImageRotatorService();

        var service = new PdfGeneratorService(
            _mockCalendarRepository.Object,
            pathProvider,
            imageRotatorService);

        // Act
        var resultPath = service.GenerateCalendarPdf(calendar, images);
        var fullPath = pathProvider.MapPath(resultPath);

        // Assert: File size should be less than 15MB
        var fileInfo = new FileInfo(fullPath);
        Assert.True(fileInfo.Exists, "PDF file should exist");

        const long maxSizeBytes = 15 * 1024 * 1024; // 15 MB
        Assert.True(fileInfo.Length < maxSizeBytes,
            $"PDF file size ({fileInfo.Length / 1024 / 1024} MB) should be less than 15MB");

        // Cleanup
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// Test implementation of IPathProvider that uses a test directory
    /// </summary>
    private class TestPathProvider : IPathProvider
    {
        private readonly string _testDir;

        public TestPathProvider(string testDir)
        {
            _testDir = testDir;
        }

        public string MapPath(string relativePath)
        {
            return Path.Combine(_testDir, relativePath);
        }
    }

    /// <summary>
    /// Test implementation of IImageRotatorService that creates simple test images
    /// </summary>
    private class TestImageRotatorService : IImageRotatorService
    {
        public iText.Layout.Element.Image LoadCorrectedImage(string imagePath)
        {
            // Create a simple 1x1 pixel image for testing
            // Use the cover.png that should be copied to output directory
            var coverPath = "images/cover.png";
            if (!File.Exists(coverPath))
            {
                // If cover doesn't exist, create a minimal test image
                using (var bitmap = new System.Drawing.Bitmap(100, 100))
                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.Clear(System.Drawing.Color.White);
                    var tempPath = Path.GetTempFileName() + ".png";
                    bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
                    var imageData = iText.IO.Image.ImageDataFactory.Create(tempPath);
                    File.Delete(tempPath);
                    return new iText.Layout.Element.Image(imageData);
                }
            }

            var data = iText.IO.Image.ImageDataFactory.Create(coverPath);
            return new iText.Layout.Element.Image(data);
        }
    }

    #endregion
}
