using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class CalendarServiceTests
{
    private readonly Mock<ICalendarRepository> _mockCalendarRepository;
    private readonly Mock<IUserSettingRepository> _mockUserSettingRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IEventDateRepository> _mockEventDateRepository;
    private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
    private readonly Mock<IHolidayRepository> _mockHolidayRepository;
    private readonly Mock<IPriceService> _mockPriceService;
    private readonly Mock<IPdfGeneratorService> _mockPdfGeneratorService;

    public CalendarServiceTests()
    {
        _mockCalendarRepository = new Mock<ICalendarRepository>();
        _mockUserSettingRepository = new Mock<IUserSettingRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockEventDateRepository = new Mock<IEventDateRepository>();
        _mockOrderItemRepository = new Mock<IOrderItemRepository>();
        _mockHolidayRepository = new Mock<IHolidayRepository>();
        _mockPriceService = new Mock<IPriceService>();
        _mockPdfGeneratorService = new Mock<IPdfGeneratorService>();

        _mockPriceService.Setup(x => x.GetPrice()).Returns(250m);
    }

    private CalendarService CreateService()
    {
        return new CalendarService(
            _mockCalendarRepository.Object,
            _mockUserSettingRepository.Object,
            _mockOrderRepository.Object,
            _mockEventDateRepository.Object,
            _mockOrderItemRepository.Object,
            _mockHolidayRepository.Object,
            _mockPriceService.Object,
            _mockPdfGeneratorService.Object);
    }

    private Calendar CreateTestCalendar(int id = 1, int userId = 1, bool isCurrent = false)
    {
        return new Calendar
        {
            Id = id,
            UserId = userId,
            LanguageId = 1,
            CountryId = 1,
            FirstDayOfWeek = DayOfWeek.Sunday,
            IsCurrent = isCurrent,
            CalendarHolidays = new List<CalendarHoliday>(),
            Images = new List<Image>()
        };
    }

    private Order CreateTestOrder(int id = 1, int userId = 1, string status = "Creating")
    {
        return new Order
        {
            Id = id,
            UserId = userId,
            Status = status,
            OrderDate = DateTime.UtcNow
        };
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_NoExistingOrder_CreatesNewOrder()
    {
        // Arrange
        var userId = 1;
        var calendar = CreateTestCalendar(userId: userId);

        _mockOrderRepository.Setup(x => x.GetOrderByStatusAsync(userId, "creating"))
            .ReturnsAsync((Order?)null);
        _mockUserSettingRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((UserSetting?)null);
        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);
        _mockOrderItemRepository.Setup(x => x.AddAsync(It.IsAny<OrderItem>()))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendar.Id))
            .ReturnsAsync(calendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(userId, calendar);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        _mockOrderRepository.Verify(x => x.AddAsync(It.Is<Order>(o => o.UserId == userId && o.Status == "Creating")), Times.Once);
        _mockCalendarRepository.Verify(x => x.AddAsync(calendar), Times.Once);
        _mockOrderItemRepository.Verify(x => x.AddAsync(It.Is<OrderItem>(oi => oi.CalendarId == calendar.Id)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ExistingOrder_UsesExistingOrder()
    {
        // Arrange
        var userId = 1;
        var calendar = CreateTestCalendar(userId: userId);
        var existingOrder = CreateTestOrder(userId: userId);

        _mockOrderRepository.Setup(x => x.GetOrderByStatusAsync(userId, "creating"))
            .ReturnsAsync(existingOrder);
        _mockCalendarRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);
        _mockOrderItemRepository.Setup(x => x.AddAsync(It.IsAny<OrderItem>()))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendar.Id))
            .ReturnsAsync(calendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(userId, calendar);

        // Assert
        Assert.NotNull(result);
        // Should NOT create a new order
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        // Should use the existing order
        _mockOrderItemRepository.Verify(x => x.AddAsync(It.Is<OrderItem>(oi => oi.OrderId == existingOrder.Id)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithUserSettings_CopiesDeliveryInfo()
    {
        // Arrange
        var userId = 1;
        var calendar = CreateTestCalendar(userId: userId);
        var userSetting = new UserSetting
        {
            UserId = userId,
            DeliveryAddress = "Test Address",
            DeliveryRaw = "Raw Data"
        };

        _mockOrderRepository.Setup(x => x.GetOrderByStatusAsync(userId, "creating"))
            .ReturnsAsync((Order?)null);
        _mockUserSettingRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userSetting);
        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                Assert.Equal("Test Address", order.DeliveryAddress);
                Assert.Equal("Raw Data", order.DeliveryRaw);
            })
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);
        _mockOrderItemRepository.Setup(x => x.AddAsync(It.IsAny<OrderItem>()))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendar.Id))
            .ReturnsAsync(calendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.CreateAsync(userId, calendar);

        // Assert
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SetsCorrectPrice()
    {
        // Arrange
        var userId = 1;
        var calendar = CreateTestCalendar(userId: userId);
        var expectedPrice = 250m;

        _mockOrderRepository.Setup(x => x.GetOrderByStatusAsync(userId, "creating"))
            .ReturnsAsync((Order?)null);
        _mockUserSettingRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((UserSetting?)null);
        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);
        _mockOrderItemRepository.Setup(x => x.AddAsync(It.IsAny<OrderItem>()))
            .Callback<OrderItem>(oi =>
            {
                Assert.Equal(expectedPrice, oi.Price);
                Assert.Equal(1, oi.Quantity);
            })
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendar.Id))
            .ReturnsAsync(calendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.CreateAsync(userId, calendar);

        // Assert
        _mockPriceService.Verify(x => x.GetPrice(), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_CalendarExists_ReturnsCalendar()
    {
        // Arrange
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId);

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync(calendar);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(calendarId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(calendarId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_CalendarDoesNotExist_ReturnsNull()
    {
        // Arrange
        var calendarId = 999;

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync((Calendar?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(calendarId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetCurrentAsync Tests

    [Fact]
    public async Task GetCurrentAsync_HasCurrentCalendar_ReturnsCurrentCalendar()
    {
        // Arrange
        var userId = 1;
        var calendars = new List<Calendar>
        {
            CreateTestCalendar(1, userId, isCurrent: false),
            CreateTestCalendar(2, userId, isCurrent: true),
            CreateTestCalendar(3, userId, isCurrent: false)
        };

        _mockCalendarRepository.Setup(x => x.GetCalendarsByUserAsync(userId))
            .ReturnsAsync(calendars);

        var service = CreateService();

        // Act
        var result = await service.GetCurrentAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCurrent);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task GetCurrentAsync_NoCurrentCalendar_ReturnsNull()
    {
        // Arrange
        var userId = 1;
        var calendars = new List<Calendar>
        {
            CreateTestCalendar(1, userId, isCurrent: false),
            CreateTestCalendar(2, userId, isCurrent: false)
        };

        _mockCalendarRepository.Setup(x => x.GetCalendarsByUserAsync(userId))
            .ReturnsAsync(calendars);

        var service = CreateService();

        // Act
        var result = await service.GetCurrentAsync(userId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UpdateCalendarAsync Tests

    [Fact]
    public async Task UpdateCalendarAsync_ValidCalendar_UpdatesCalendar()
    {
        // Arrange
        var userId = 1;
        var existingCalendar = CreateTestCalendar(1, userId);
        existingCalendar.LanguageId = 1;
        existingCalendar.FirstDayOfWeek = DayOfWeek.Sunday;

        var updatedCalendar = CreateTestCalendar(1, userId);
        updatedCalendar.LanguageId = 2;
        updatedCalendar.FirstDayOfWeek = DayOfWeek.Monday;

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(1))
            .ReturnsAsync(existingCalendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.UpdateCalendarAsync(userId, updatedCalendar);

        // Assert
        Assert.True(result);
        Assert.Equal(2, existingCalendar.LanguageId);
        Assert.Equal(DayOfWeek.Monday, existingCalendar.FirstDayOfWeek);
        _mockCalendarRepository.Verify(x => x.UpdateAsync(existingCalendar), Times.Once);
    }

    [Fact]
    public async Task UpdateCalendarAsync_CalendarDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var calendar = CreateTestCalendar(999, userId);

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(999))
            .ReturnsAsync((Calendar?)null);

        var service = CreateService();

        // Act
        var result = await service.UpdateCalendarAsync(userId, calendar);

        // Assert
        Assert.False(result);
        _mockCalendarRepository.Verify(x => x.UpdateAsync(It.IsAny<Calendar>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCalendarAsync_WrongUser_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var wrongUserId = 2;
        var calendar = CreateTestCalendar(1, wrongUserId);

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(1))
            .ReturnsAsync(calendar);

        var service = CreateService();

        // Act
        var result = await service.UpdateCalendarAsync(userId, calendar);

        // Assert
        Assert.False(result);
        _mockCalendarRepository.Verify(x => x.UpdateAsync(It.IsAny<Calendar>()), Times.Never);
    }

    #endregion

    #region MakeCurrentAsync Tests

    [Fact]
    public async Task MakeCurrentAsync_ValidCalendar_SetsIsCurrent()
    {
        // Arrange
        var userId = 1;
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId, userId);
        calendar.IsCurrent = false;

        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendarId))
            .ReturnsAsync(calendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.MakeCurrentAsync(userId, calendarId);

        // Assert
        Assert.True(calendar.IsCurrent);
        _mockCalendarRepository.Verify(x => x.UpdateAsync(calendar), Times.Once);
    }

    [Fact]
    public async Task MakeCurrentAsync_WrongUser_DoesNotUpdate()
    {
        // Arrange
        var userId = 1;
        var wrongUserId = 2;
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId, wrongUserId);

        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendarId))
            .ReturnsAsync(calendar);

        var service = CreateService();

        // Act
        await service.MakeCurrentAsync(userId, calendarId);

        // Assert
        _mockCalendarRepository.Verify(x => x.UpdateAsync(It.IsAny<Calendar>()), Times.Never);
    }

    #endregion

    #region MakeNotCurrentAsync Tests

    [Fact]
    public async Task MakeNotCurrentAsync_ValidCalendar_SetsIsCurrentToFalse()
    {
        // Arrange
        var userId = 1;
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId, userId);
        calendar.IsCurrent = true;

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync(calendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.MakeNotCurrentAsync(userId, calendarId);

        // Assert
        Assert.False(calendar.IsCurrent);
        _mockCalendarRepository.Verify(x => x.UpdateAsync(calendar), Times.Once);
    }

    [Fact]
    public async Task MakeNotCurrentAsync_WrongUser_DoesNotUpdate()
    {
        // Arrange
        var userId = 1;
        var wrongUserId = 2;
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId, wrongUserId);

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync(calendar);

        var service = CreateService();

        // Act
        await service.MakeNotCurrentAsync(userId, calendarId);

        // Assert
        _mockCalendarRepository.Verify(x => x.UpdateAsync(It.IsAny<Calendar>()), Times.Never);
    }

    #endregion

    #region GeneratePdfAsync Tests

    [Fact]
    public async Task GeneratePdfAsync_ValidCalendar_GeneratesPdf()
    {
        // Arrange
        var userId = 1;
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId, userId);
        calendar.CalendarHolidays = new List<CalendarHoliday>
        {
            new CalendarHoliday { CalendarId = calendarId, HolidayId = 1 }
        };

        var pdfPath = "/path/to/calendar.pdf";

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync(calendar);
        _mockPdfGeneratorService.Setup(x => x.GeneratePdfAsync(calendarId))
            .ReturnsAsync(pdfPath);
        _mockCalendarRepository.Setup(x => x.SaveFileAsync(calendarId, pdfPath))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.GeneratePdfAsync(userId, calendarId);

        // Assert
        _mockPdfGeneratorService.Verify(x => x.GeneratePdfAsync(calendarId), Times.Once);
        _mockCalendarRepository.Verify(x => x.SaveFileAsync(calendarId, pdfPath), Times.Once);
    }

    [Fact]
    public async Task GeneratePdfAsync_CalendarWithoutHolidays_AssignsHolidaysAndEvents()
    {
        // Arrange
        var userId = 1;
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId, userId);
        calendar.CalendarHolidays = new List<CalendarHoliday>(); // Empty

        var eventDates = new List<EventDate>
        {
            new EventDate { Id = 1, Date = DateTime.UtcNow, Description = "Event" }
        };
        var holidays = new List<Holiday>
        {
            new Holiday { Id = 1, CountryId = calendar.CountryId }
        };

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync(calendar);
        _mockEventDateRepository.Setup(x => x.GetAllByUserIdAsync(userId))
            .ReturnsAsync(eventDates);
        _mockHolidayRepository.Setup(x => x.GetAllByCoutryIdAsync(calendar.CountryId))
            .ReturnsAsync(holidays);
        _mockCalendarRepository.Setup(x => x.AssignEventDatesAsync(calendarId, eventDates))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.AssignHolidays(calendarId, holidays))
            .Returns(Task.CompletedTask);
        _mockPdfGeneratorService.Setup(x => x.GeneratePdfAsync(calendarId))
            .ReturnsAsync("/path/to/calendar.pdf");
        _mockCalendarRepository.Setup(x => x.SaveFileAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.GeneratePdfAsync(userId, calendarId);

        // Assert
        _mockEventDateRepository.Verify(x => x.GetAllByUserIdAsync(userId), Times.Once);
        _mockHolidayRepository.Verify(x => x.GetAllByCoutryIdAsync(calendar.CountryId), Times.Once);
        _mockCalendarRepository.Verify(x => x.AssignEventDatesAsync(calendarId, eventDates), Times.Once);
        _mockCalendarRepository.Verify(x => x.AssignHolidays(calendarId, holidays), Times.Once);
    }

    [Fact]
    public async Task GeneratePdfAsync_CalendarDoesNotExist_DoesNothing()
    {
        // Arrange
        var userId = 1;
        var calendarId = 999;

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync((Calendar?)null);

        var service = CreateService();

        // Act
        await service.GeneratePdfAsync(userId, calendarId);

        // Assert
        _mockPdfGeneratorService.Verify(x => x.GeneratePdfAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        // Arrange
        var calendarId = 1;

        _mockCalendarRepository.Setup(x => x.DeleteAsync(calendarId))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.DeleteAsync(calendarId);

        // Assert
        _mockCalendarRepository.Verify(x => x.DeleteAsync(calendarId), Times.Once);
    }

    #endregion

    #region UpdatePreviewPathAsync Tests

    [Fact]
    public async Task UpdatePreviewPathAsync_CallsRepository()
    {
        // Arrange
        var calendarId = 1;
        var previewPath = "/path/to/preview.jpg";

        _mockCalendarRepository.Setup(x => x.UpdatePreviewPathAsync(calendarId, previewPath))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.UpdatePreviewPathAsync(calendarId, previewPath);

        // Assert
        _mockCalendarRepository.Verify(x => x.UpdatePreviewPathAsync(calendarId, previewPath), Times.Once);
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserCalendars()
    {
        // Arrange
        var userId = 1;
        var calendars = new List<Calendar>
        {
            CreateTestCalendar(1, userId),
            CreateTestCalendar(2, userId),
            CreateTestCalendar(3, userId)
        };

        _mockCalendarRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(calendars);

        var service = CreateService();

        // Act
        var result = await service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    #endregion

    #region Edge Cases and Additional Coverage Tests

    [Fact]
    public async Task CreateAsync_MultipleCalls_ReusesExistingOrder()
    {
        // Arrange
        var userId = 1;
        var calendar1 = CreateTestCalendar(userId: userId);
        var calendar2 = CreateTestCalendar(2, userId: userId);
        var existingOrder = CreateTestOrder(userId: userId);

        _mockOrderRepository.Setup(x => x.GetOrderByStatusAsync(userId, "creating"))
            .ReturnsAsync(existingOrder);
        _mockCalendarRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);
        _mockOrderItemRepository.Setup(x => x.AddAsync(It.IsAny<OrderItem>()))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => id == calendar1.Id ? calendar1 : calendar2);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.CreateAsync(userId, calendar1);
        await service.CreateAsync(userId, calendar2);

        // Assert
        // Should create only ONE order for both calendars
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _mockOrderItemRepository.Verify(x => x.AddAsync(It.IsAny<OrderItem>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetCurrentAsync_EmptyCalendarList_ReturnsNull()
    {
        // Arrange
        var userId = 1;
        var calendars = new List<Calendar>();

        _mockCalendarRepository.Setup(x => x.GetCalendarsByUserAsync(userId))
            .ReturnsAsync(calendars);

        var service = CreateService();

        // Act
        var result = await service.GetCurrentAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentAsync_MultipleCurrentCalendars_ReturnsNewest()
    {
        // Arrange
        var userId = 1;
        var calendars = new List<Calendar>
        {
            CreateTestCalendar(1, userId, isCurrent: true),
            CreateTestCalendar(2, userId, isCurrent: true),
            CreateTestCalendar(3, userId, isCurrent: true)
        };

        _mockCalendarRepository.Setup(x => x.GetCalendarsByUserAsync(userId))
            .ReturnsAsync(calendars);

        var service = CreateService();

        // Act
        var result = await service.GetCurrentAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id); // Newest (highest ID)
    }

    [Fact]
    public async Task MakeCurrentAsync_CalendarNotFound_DoesNothing()
    {
        // Arrange
        var userId = 1;
        var calendarId = 999;

        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendarId))
            .ReturnsAsync((Calendar?)null);

        var service = CreateService();

        // Act
        await service.MakeCurrentAsync(userId, calendarId);

        // Assert
        _mockCalendarRepository.Verify(x => x.UpdateAsync(It.IsAny<Calendar>()), Times.Never);
    }

    [Fact]
    public async Task MakeNotCurrentAsync_CalendarNotFound_DoesNothing()
    {
        // Arrange
        var userId = 1;
        var calendarId = 999;

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync((Calendar?)null);

        var service = CreateService();

        // Act
        await service.MakeNotCurrentAsync(userId, calendarId);

        // Assert
        _mockCalendarRepository.Verify(x => x.UpdateAsync(It.IsAny<Calendar>()), Times.Never);
    }

    [Fact]
    public async Task GeneratePdfAsync_AlreadyHasHolidays_SkipsAssignment()
    {
        // Arrange
        var userId = 1;
        var calendarId = 1;
        var calendar = CreateTestCalendar(calendarId, userId);
        calendar.CalendarHolidays = new List<CalendarHoliday>
        {
            new CalendarHoliday { CalendarId = calendarId, HolidayId = 1 },
            new CalendarHoliday { CalendarId = calendarId, HolidayId = 2 }
        };

        var pdfPath = "/path/to/calendar.pdf";

        _mockCalendarRepository.Setup(x => x.GetFullCalendarAsync(calendarId))
            .ReturnsAsync(calendar);
        _mockPdfGeneratorService.Setup(x => x.GeneratePdfAsync(calendarId))
            .ReturnsAsync(pdfPath);
        _mockCalendarRepository.Setup(x => x.SaveFileAsync(calendarId, pdfPath))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.GeneratePdfAsync(userId, calendarId);

        // Assert
        // Should NOT assign holidays since calendar already has them
        _mockEventDateRepository.Verify(x => x.GetAllByUserIdAsync(It.IsAny<int>()), Times.Never);
        _mockHolidayRepository.Verify(x => x.GetAllByCoutryIdAsync(It.IsAny<int>()), Times.Never);
        _mockCalendarRepository.Verify(x => x.AssignEventDatesAsync(It.IsAny<int>(), It.IsAny<IList<EventDate>>()), Times.Never);
        _mockCalendarRepository.Verify(x => x.AssignHolidays(It.IsAny<int>(), It.IsAny<IList<Holiday>>()), Times.Never);
    }

    [Fact]
    public async Task GetByUserIdAsync_NoCalendars_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        var calendars = new List<Calendar>();

        _mockCalendarRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(calendars);

        var service = CreateService();

        // Act
        var result = await service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_PriceServiceCalled_UsesCorrectPrice()
    {
        // Arrange
        var userId = 1;
        var calendar = CreateTestCalendar(userId: userId);
        var testPrice = 999.99m;

        _mockPriceService.Setup(x => x.GetPrice()).Returns(testPrice);
        _mockOrderRepository.Setup(x => x.GetOrderByStatusAsync(userId, "creating"))
            .ReturnsAsync((Order?)null);
        _mockUserSettingRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((UserSetting?)null);
        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _mockCalendarRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        OrderItem? capturedOrderItem = null;
        _mockOrderItemRepository.Setup(x => x.AddAsync(It.IsAny<OrderItem>()))
            .Callback<OrderItem>(oi => capturedOrderItem = oi)
            .Returns(Task.CompletedTask);

        _mockCalendarRepository.Setup(x => x.GetByIdAsync(calendar.Id))
            .ReturnsAsync(calendar);
        _mockCalendarRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.CreateAsync(userId, calendar);

        // Assert
        Assert.NotNull(capturedOrderItem);
        Assert.Equal(testPrice, capturedOrderItem.Price);
    }

    #endregion
}
