using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class TestPromptProfile : Profile
{
    public TestPromptProfile()
    {
        CreateMap<TestPrompt, TestPromptDto>();
    }
}
