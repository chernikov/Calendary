using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class SynthesisProfile : Profile
{
    public SynthesisProfile()
    {
        CreateMap<Synthesis, SynthesisDto>();
    }
}
