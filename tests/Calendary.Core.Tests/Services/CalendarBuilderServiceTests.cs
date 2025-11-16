using System.Globalization;
using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class CalendarBuilderServiceTests
{
    private readonly Mock<IHolidayRepository> _holidayRepositoryMock;

    public CalendarBuilderServiceTests()
    {
        _holidayRepositoryMock = new Mock<IHolidayRepository>();
        _holidayRepositoryMock
            .Setup(r => r.GetAllByCoutryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<Holiday>());
    }

    [Fact]
    public async Task BuildCalendar2026Async_BuildsCalendarAndGrid()
    {
        var service = CreateService();

        var calendar = await service.BuildCalendar2026Async(CreateImages(), "uk-UA", DayOfWeek.Monday);

        Assert.Equal(2026, calendar.Year);
        Assert.Equal(DayOfWeek.Monday, calendar.FirstDayOfWeek);
        Assert.Equal(12, calendar.Images.Count);
        Assert.NotNull(service.LastGeneratedGrid);
        Assert.Equal(12, service.LastGeneratedGrid!.Months.Count);
        Assert.Equal("uk-UA", service.LastGeneratedGrid.Language);
    }

    [Fact]
    public async Task ApplyHolidaysAsync_FlagsHolidayCells()
    {
        var holidays = new[]
        {
            new Holiday
            {
                Id = 1,
                Name = "New Year",
                Date = new DateTime(2026, 1, 1),
                CountryId = Country.Ukraine.Id
            },
            new Holiday
            {
                Id = 2,
                Name = "Old Date",
                Date = new DateTime(2025, 12, 31),
                CountryId = Country.Ukraine.Id
            }
        };

        var service = CreateService(holidays);

        var calendar = await service.BuildCalendar2026Async(CreateImages(), "uk-UA", DayOfWeek.Monday);

        var grid = service.LastGeneratedGrid!;
        var januaryHoliday = grid.Months
            .First(m => m.Month == 1)
            .Weeks
            .SelectMany(w => w)
            .First(d => d.IsCurrentMonth && d.Date.Day == 1);

        Assert.True(januaryHoliday.IsHoliday);
        Assert.Equal("New Year", januaryHoliday.HolidayName);
        Assert.Single(calendar.CalendarHolidays);
    }

    [Fact]
    public async Task AssignImages_SetsImageUrlsForMonths()
    {
        var service = CreateService();

        await service.BuildCalendar2026Async(CreateImages(), "en-US", DayOfWeek.Sunday);

        var may = service.LastGeneratedGrid!.Months.First(m => m.Month == 5);

        Assert.Equal("image-5.png", may.ImageUrl);
    }

    [Fact]
    public async Task BuildCalendar2026Async_UsesLocalizationForMonthAndDayNames()
    {
        var service = CreateService();
        var language = "en-US";
        var expectedCulture = new CultureInfo(language);
        var expectedDayNames = ReorderDays(expectedCulture, DayOfWeek.Sunday);

        await service.BuildCalendar2026Async(CreateImages(), language, DayOfWeek.Sunday);

        var grid = service.LastGeneratedGrid!;
        Assert.Equal(expectedCulture.DateTimeFormat.MonthNames[0], grid.Months.First().Name);
        Assert.Equal(expectedDayNames, grid.DayNames);
    }

    [Fact]
    public async Task GenerateMonthGrid_AlignsWithStartDay()
    {
        var service = CreateService();
        await service.BuildCalendar2026Async(CreateImages(), "uk-UA", DayOfWeek.Monday);

        var january = service.GenerateMonthGrid(2026, 1);
        var firstWeek = january.Weeks.First();

        Assert.False(firstWeek[0].IsCurrentMonth);
        Assert.True(firstWeek[3].IsCurrentMonth);
        Assert.Equal(1, firstWeek[3].Date.Day);
        Assert.Equal(DayOfWeek.Thursday, firstWeek[3].Date.DayOfWeek);
    }

    private CalendarBuilderService CreateService(IEnumerable<Holiday>? holidays = null)
    {
        if (holidays is not null)
        {
            _holidayRepositoryMock
                .Setup(r => r.GetAllByCoutryIdAsync(It.IsAny<int>()))
                .ReturnsAsync(holidays);
        }

        return new CalendarBuilderService(_holidayRepositoryMock.Object);
    }

    private static Dictionary<int, string> CreateImages()
    {
        return Enumerable.Range(1, 12).ToDictionary(m => m, m => $"image-{m}.png");
    }

    private static string[] ReorderDays(CultureInfo culture, DayOfWeek startDay)
    {
        var dayNames = culture.DateTimeFormat.AbbreviatedDayNames;
        var reordered = new string[7];
        var startIndex = (int)startDay;

        for (int i = 0; i < 7; i++)
        {
            reordered[i] = dayNames[(startIndex + i) % 7];
        }

        return reordered;
    }
}
