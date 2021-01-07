using ApiProject.Entities;
using ApiProject.Models.User;
using AutoMapper;

namespace ApiProject.Mapper
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<User, CreateUserRequest>();
            CreateMap<User, UpdateUserRequest>();
        }
    }
}
