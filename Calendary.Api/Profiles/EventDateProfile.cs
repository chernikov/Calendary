using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class EventDateProfile : Profile
{
    public EventDateProfile()
    {
        CreateMap<EventDate, EventDateDto>();
    }
}

