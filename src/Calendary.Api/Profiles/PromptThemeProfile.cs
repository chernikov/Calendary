using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class PromptThemeProfile : Profile
{
    public PromptThemeProfile()
    {
        CreateMap<PromptTheme, PromptThemeDto>().ReverseMap();
    }
}
