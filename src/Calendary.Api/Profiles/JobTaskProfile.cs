using AutoMapper;
using Calendary.Core.Dto;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class JobTaskProfile : Profile
{
    public JobTaskProfile()
    {
        CreateMap<JobTask, JobTaskDto>();
    }
}
