using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles
{
    public class ImageProfile : Profile
    {
        public ImageProfile()
        {
            CreateMap<Image, ImageDto>();
        }
    }
}
