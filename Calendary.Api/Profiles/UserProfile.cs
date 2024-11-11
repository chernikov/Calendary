using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserLoginDto, User>();
        CreateMap<UserInfoDto, User>();
        CreateMap<User, UserDto>();
        CreateMap<User, UserInfoDto>();
    }
}
