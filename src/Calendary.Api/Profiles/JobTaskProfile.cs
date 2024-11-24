using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;
using Calendary.Model.Messages;

namespace Calendary.Api.Profiles;

public class JobTaskProfile : Profile
{
    public JobTaskProfile()
    {
        CreateMap<JobTask, JobTaskDto>();
        CreateMap<JobTask, JobTaskMessage>();
    }
}
