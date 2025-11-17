using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class PromptSeedProfile : Profile
{
    public PromptSeedProfile()
    {
        CreateMap<PromptSeed, PromptSeedDto>().ReverseMap();
    }
}

