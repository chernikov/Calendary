using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class HolidayProfile : Profile
{
    public HolidayProfile()
    {
        CreateMap<Holiday, HolidayDto>();
        CreateMap<HolidayDto, Holiday>();
    }
}
