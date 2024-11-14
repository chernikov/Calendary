namespace Calendary.Api.Dtos
{
    public class SummaryOrderDto
    {
        public class OrderItemDto
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public CalendarDto Calendar { get; set; } = null!;
        }

        public class CalendarDto
        {
            public int Id { get; set; }
        }

        public class UserDto
        {
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public bool IsEmailConfirmed { get; set; }
            public bool IsPhoneNumberConfirmed { get; set; }
        }

        public int Id { get; set; }
        public IEnumerable<OrderItemDto> OrderItems { get; set; } = Array.Empty<OrderItemDto>();
        public decimal Sum { get; set; }
        public string? DeliveryAddress { get; set; }
        public UserDto User { get; set; }

        public bool IsPaid { get; set; }
    }
}
