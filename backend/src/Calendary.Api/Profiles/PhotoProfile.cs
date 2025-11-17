using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class PhotoProfile : Profile
{
    public PhotoProfile()
    {
        CreateMap<Photo, PhotoDto>();
    }
}
