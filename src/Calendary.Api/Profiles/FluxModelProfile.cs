﻿using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class FluxModelProfile : Profile
{
    public FluxModelProfile()
    {
        CreateMap<FluxModel, FluxModelDto>().ReverseMap();

        CreateMap<FluxModel, AdminFluxModelDto>();
    }
}
