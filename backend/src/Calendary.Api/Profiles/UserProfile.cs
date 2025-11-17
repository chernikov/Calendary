using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Api.Dtos.Admin;
using Calendary.Model;

namespace Calendary.Api.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserLoginDto, User>();
        CreateMap<UserRegisterDto, User>();
        CreateMap<UserInfoDto, User>();
        CreateMap<User, UserDto>();
        CreateMap<User, UserInfoDto>();

        CreateMap<User, AdminUserDto>();
        CreateMap<AdminUserDto, User>()
            .ForMember(p => p.CreatedByAdmin, opt => opt.MapFrom(_ => true));
        CreateMap<AdminCreateUserDto, User>()
            .ForMember(p => p.CreatedByAdmin, opt => opt.MapFrom(_ => true));

    }
}
