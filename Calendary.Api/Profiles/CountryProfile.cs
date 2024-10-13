using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class CountryProfile : Profile
{
    public CountryProfile()
    {
        CreateMap<Country, CountryDto>();
        CreateMap<CountryDto, Country>();
    }
}
