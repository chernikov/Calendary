using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class TrainingProfile : Profile
{
    public TrainingProfile()
    {
        CreateMap<Training, TrainingDto>();
    }
}
