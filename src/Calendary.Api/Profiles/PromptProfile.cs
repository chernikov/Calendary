using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles
{
    public class PromptProfile : Profile
    {
        public PromptProfile()
        {
            CreateMap<Prompt, PromptDto>().ReverseMap();
        }
    }
}
