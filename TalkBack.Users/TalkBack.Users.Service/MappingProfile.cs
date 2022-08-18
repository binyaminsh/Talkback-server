using AutoMapper;
using TalkBack.Users.Service.Dtos;
using TalkBack.Users.Service.Entities;

namespace TalkBack.Users.Service
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UpdateUserDto>().ReverseMap();
        }
    }
}
