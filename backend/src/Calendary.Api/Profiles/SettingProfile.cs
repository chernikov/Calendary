using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles
{
    public class SettingProfile : Profile
    {
        public SettingProfile()
        {
            CreateMap<UserSetting, SettingDto>();
            CreateMap<SettingDto, UserSetting>();
        }
    }
}
