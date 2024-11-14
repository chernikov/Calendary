using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(p => p.Items, opt => opt.MapFrom(r => r.OrderItems));

        CreateMap<Order, SummaryOrderDto>()
            .ForMember(p => p.Sum, opt => opt.MapFrom(r => r.OrderItems.Sum(item => item.Price * item.Quantity)));
        CreateMap<OrderItem, SummaryOrderDto.OrderItemDto>();
        CreateMap<Calendar, SummaryOrderDto.CalendarDto>();
        CreateMap<User, SummaryOrderDto.UserDto>();
    }
}
