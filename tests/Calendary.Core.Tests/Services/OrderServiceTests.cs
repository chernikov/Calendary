using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockOrderItemRepository = new Mock<IOrderItemRepository>();
    }

    private Order CreateTestOrder(int id = 1, int userId = 1, string status = "creating")
    {
        return new Order
        {
            Id = id,
            UserId = userId,
            Status = status,
            OrderDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            IsPaid = false,
            DeliveryAddress = "Test Address",
            DeliveryRaw = "{\"city\":\"Kyiv\",\"postOffice\":\"1\"}",
            Comment = "Test comment",
            OrderItems = new List<OrderItem>()
        };
    }

    private OrderItem CreateTestOrderItem(int id = 1, int orderId = 1, int calendarId = 1, int quantity = 1)
    {
        return new OrderItem
        {
            Id = id,
            OrderId = orderId,
            CalendarId = calendarId,
            Quantity = quantity,
            Price = 100.00m,
            Calendar = new Calendar
            {
                Id = calendarId,
                Year = 2024,
                FilePath = "uploads/calendar.pdf"
            }
        };
    }

    #region GetOrderAsync Tests

    [Fact]
    public async Task GetOrderAsync_OrderExists_ReturnsOrder()
    {
        // Arrange
        var orderId = 1;
        var expectedOrder = CreateTestOrder(orderId);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(expectedOrder);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public async Task GetOrderAsync_OrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetOrderAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetFullCreatingOrderAsync Tests

    [Fact]
    public async Task GetFullCreatingOrderAsync_CreatingOrderExists_ReturnsOrder()
    {
        // Arrange
        var userId = 1;
        var expectedOrder = CreateTestOrder(userId: userId, status: "creating");

        _mockOrderRepository.Setup(x => x.GetFullOrderByStatusAsync(userId, "Creating"))
            .ReturnsAsync(expectedOrder);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetFullCreatingOrderAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("creating", result.Status);
    }

    [Fact]
    public async Task GetFullCreatingOrderAsync_NoCreatingOrder_ReturnsNull()
    {
        // Arrange
        _mockOrderRepository.Setup(x => x.GetFullOrderByStatusAsync(It.IsAny<int>(), "Creating"))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetFullCreatingOrderAsync(1);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetFullOrderAsync Tests

    [Fact]
    public async Task GetFullOrderAsync_OrderExists_ReturnsFullOrder()
    {
        // Arrange
        var orderId = 1;
        var expectedOrder = CreateTestOrder(orderId);

        _mockOrderRepository.Setup(x => x.GetFullOrderAsync(orderId))
            .ReturnsAsync(expectedOrder);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetFullOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public async Task GetFullOrderAsync_OrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockOrderRepository.Setup(x => x.GetFullOrderAsync(It.IsAny<int>()))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetFullOrderAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetOrderByIdAsync Tests

    [Fact]
    public async Task GetOrderByIdAsync_OrderExists_ReturnsOrder()
    {
        // Arrange
        var orderId = 1;
        var expectedOrder = CreateTestOrder(orderId);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(expectedOrder);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetOrderByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public async Task GetOrderByIdAsync_OrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetOrderByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetOrderItemByIdAsync Tests

    [Fact]
    public async Task GetOrderItemByIdAsync_ItemExists_ReturnsOrderItem()
    {
        // Arrange
        var itemId = 1;
        var expectedItem = CreateTestOrderItem(itemId);

        _mockOrderItemRepository.Setup(x => x.GetByIdAsync(itemId))
            .ReturnsAsync(expectedItem);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetOrderItemByIdAsync(itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(itemId, result.Id);
    }

    [Fact]
    public async Task GetOrderItemByIdAsync_ItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockOrderItemRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((OrderItem)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.GetOrderItemByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region DeleteOrderItemAsync Tests

    [Fact]
    public async Task DeleteOrderItemAsync_ValidItemId_CallsRepositoryDelete()
    {
        // Arrange
        var itemId = 1;
        _mockOrderItemRepository.Setup(x => x.DeleteAsync(itemId))
            .Returns(Task.CompletedTask);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.DeleteOrderItemAsync(itemId);

        // Assert
        _mockOrderItemRepository.Verify(x => x.DeleteAsync(itemId), Times.Once);
    }

    #endregion

    #region UpdateOrderItemAsync Tests

    [Fact]
    public async Task UpdateOrderItemAsync_ItemExists_UpdatesQuantity()
    {
        // Arrange
        var existingItem = CreateTestOrderItem(quantity: 1);
        var updatedItem = CreateTestOrderItem(quantity: 5);

        _mockOrderItemRepository.Setup(x => x.GetByIdAsync(updatedItem.Id))
            .ReturnsAsync(existingItem);
        _mockOrderItemRepository.Setup(x => x.UpdateAsync(It.IsAny<OrderItem>()))
            .Returns(Task.CompletedTask);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderItemAsync(updatedItem);

        // Assert
        Assert.Equal(5, existingItem.Quantity);
        _mockOrderItemRepository.Verify(x => x.UpdateAsync(existingItem), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_ItemDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedItem = CreateTestOrderItem();
        _mockOrderItemRepository.Setup(x => x.GetByIdAsync(updatedItem.Id))
            .ReturnsAsync((OrderItem)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderItemAsync(updatedItem);

        // Assert
        _mockOrderItemRepository.Verify(x => x.UpdateAsync(It.IsAny<OrderItem>()), Times.Never);
    }

    #endregion

    #region OrderItemsCountAsync Tests

    [Fact]
    public async Task OrderItemsCountAsync_OrderWithItems_ReturnsCorrectCount()
    {
        // Arrange
        var userId = 1;
        var order = CreateTestOrder(userId: userId);
        order.OrderItems.Add(CreateTestOrderItem(1, order.Id, 1, 2));
        order.OrderItems.Add(CreateTestOrderItem(2, order.Id, 2, 3));

        _mockOrderRepository.Setup(x => x.GetOrderWithItemsAsync(userId, "creating"))
            .ReturnsAsync(order);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.OrderItemsCountAsync(userId);

        // Assert
        Assert.Equal(5, result); // 2 + 3 = 5
    }

    [Fact]
    public async Task OrderItemsCountAsync_NoOrder_ReturnsZero()
    {
        // Arrange
        _mockOrderRepository.Setup(x => x.GetOrderWithItemsAsync(It.IsAny<int>(), "creating"))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.OrderItemsCountAsync(1);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task OrderItemsCountAsync_OrderWithItemsWithoutFilePath_ReturnsZero()
    {
        // Arrange
        var userId = 1;
        var order = CreateTestOrder(userId: userId);
        var item = CreateTestOrderItem(1, order.Id, 1, 2);
        item.Calendar.FilePath = null; // No file path
        order.OrderItems.Add(item);

        _mockOrderRepository.Setup(x => x.GetOrderWithItemsAsync(userId, "creating"))
            .ReturnsAsync(order);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var result = await service.OrderItemsCountAsync(userId);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region UpdateOrderDeliveryAsync Tests

    [Fact]
    public async Task UpdateOrderDeliveryAsync_OrderExists_UpdatesDeliveryInfo()
    {
        // Arrange
        var existingOrder = CreateTestOrder();
        existingOrder.DeliveryAddress = "Old Address";
        existingOrder.DeliveryRaw = "{}";

        var updatedOrder = CreateTestOrder();
        updatedOrder.DeliveryAddress = "New Address";
        updatedOrder.DeliveryRaw = "{\"city\":\"Lviv\"}";

        _mockOrderRepository.Setup(x => x.GetByIdAsync(updatedOrder.Id))
            .ReturnsAsync(existingOrder);
        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderDeliveryAsync(updatedOrder);

        // Assert
        Assert.Equal("New Address", existingOrder.DeliveryAddress);
        Assert.Equal("{\"city\":\"Lviv\"}", existingOrder.DeliveryRaw);
        _mockOrderRepository.Verify(x => x.UpdateAsync(existingOrder), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderDeliveryAsync_OrderDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedOrder = CreateTestOrder();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(updatedOrder.Id))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderDeliveryAsync(updatedOrder);

        // Assert
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    #endregion

    #region UpdateOrderStatusAsync Tests

    [Fact]
    public async Task UpdateOrderStatusAsync_OrderExists_UpdatesStatusAndIsPaid()
    {
        // Arrange
        var existingOrder = CreateTestOrder();
        existingOrder.IsPaid = false;
        existingOrder.Status = "creating";
        var originalUpdateDate = existingOrder.UpdatedDate;

        var updatedOrder = CreateTestOrder();
        updatedOrder.IsPaid = true;
        updatedOrder.Status = "completed";

        _mockOrderRepository.Setup(x => x.GetByIdAsync(updatedOrder.Id))
            .ReturnsAsync(existingOrder);
        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderStatusAsync(updatedOrder);

        // Assert
        Assert.True(existingOrder.IsPaid);
        Assert.Equal("completed", existingOrder.Status);
        Assert.NotEqual(originalUpdateDate, existingOrder.UpdatedDate);
        _mockOrderRepository.Verify(x => x.UpdateAsync(existingOrder), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_OrderDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedOrder = CreateTestOrder();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(updatedOrder.Id))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderStatusAsync(updatedOrder);

        // Assert
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    #endregion

    #region UpdateOrderCommentAsync Tests

    [Fact]
    public async Task UpdateOrderCommentAsync_OrderExists_UpdatesComment()
    {
        // Arrange
        var existingOrder = CreateTestOrder();
        existingOrder.Comment = "Old comment";

        var updatedOrder = CreateTestOrder();
        updatedOrder.Comment = "New comment";

        _mockOrderRepository.Setup(x => x.GetByIdAsync(updatedOrder.Id))
            .ReturnsAsync(existingOrder);
        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderCommentAsync(updatedOrder);

        // Assert
        Assert.Equal("New comment", existingOrder.Comment);
        _mockOrderRepository.Verify(x => x.UpdateAsync(existingOrder), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderCommentAsync_OrderDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedOrder = CreateTestOrder();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(updatedOrder.Id))
            .ReturnsAsync((Order)null);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        await service.UpdateOrderCommentAsync(updatedOrder);

        // Assert
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    #endregion

    #region GetOrdersWithPagingAsync Tests

    [Fact]
    public async Task GetOrdersWithPagingAsync_ValidParameters_ReturnsOrdersAndCount()
    {
        // Arrange
        var userId = 1;
        var orders = new List<Order>
        {
            CreateTestOrder(1, userId),
            CreateTestOrder(2, userId),
            CreateTestOrder(3, userId)
        };
        var expectedResult = (orders, 3);

        _mockOrderRepository.Setup(x => x.GetOrdersWithPagingAsync(userId, 1, 10))
            .ReturnsAsync(expectedResult);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var (resultOrders, totalCount) = await service.GetOrdersWithPagingAsync(userId, 1, 10);

        // Assert
        Assert.NotNull(resultOrders);
        Assert.Equal(3, resultOrders.Count);
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task GetOrdersWithPagingAsync_EmptyResult_ReturnsEmptyListAndZero()
    {
        // Arrange
        var expectedResult = (new List<Order>(), 0);
        _mockOrderRepository.Setup(x => x.GetOrdersWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedResult);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var (resultOrders, totalCount) = await service.GetOrdersWithPagingAsync(1, 1, 10);

        // Assert
        Assert.NotNull(resultOrders);
        Assert.Empty(resultOrders);
        Assert.Equal(0, totalCount);
    }

    #endregion

    #region GetAllOrdersWithPagingAsync Tests

    [Fact]
    public async Task GetAllOrdersWithPagingAsync_ValidParameters_ReturnsOrdersAndCount()
    {
        // Arrange
        var orders = new List<Order>
        {
            CreateTestOrder(1, 1),
            CreateTestOrder(2, 2),
            CreateTestOrder(3, 3)
        };
        var expectedResult = (orders, 3);

        _mockOrderRepository.Setup(x => x.GetAllOrdersWithPagingAsync(1, 10))
            .ReturnsAsync(expectedResult);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var (resultOrders, totalCount) = await service.GetAllOrdersWithPagingAsync(1, 10);

        // Assert
        Assert.NotNull(resultOrders);
        Assert.Equal(3, resultOrders.Count);
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task GetAllOrdersWithPagingAsync_EmptyResult_ReturnsEmptyListAndZero()
    {
        // Arrange
        var expectedResult = (new List<Order>(), 0);
        _mockOrderRepository.Setup(x => x.GetAllOrdersWithPagingAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedResult);

        var service = new OrderService(_mockOrderRepository.Object, _mockOrderItemRepository.Object);

        // Act
        var (resultOrders, totalCount) = await service.GetAllOrdersWithPagingAsync(1, 10);

        // Assert
        Assert.NotNull(resultOrders);
        Assert.Empty(resultOrders);
        Assert.Equal(0, totalCount);
    }

    #endregion
}
