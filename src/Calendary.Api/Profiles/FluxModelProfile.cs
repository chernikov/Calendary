using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Api.Dtos.Admin;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class FluxModelProfile : Profile
{
    public FluxModelProfile()
    {
        CreateMap<FluxModel, FluxModelDto>();
        CreateMap<FluxModelDto, FluxModel>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Trainings, opt => opt.Ignore())
            .ForMember(dest => dest.Jobs, opt => opt.Ignore());

        CreateMap<FluxModel, AdminFluxModelDto>();

        CreateMap<CreateFluxModelDto, FluxModel>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Trainings, opt => opt.Ignore())
            .ForMember(dest => dest.Jobs, opt => opt.Ignore());
    }
}
