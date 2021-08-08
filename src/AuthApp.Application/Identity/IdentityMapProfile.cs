using AuthApp.Domian;
using AuthApp.Domian.Identity;
using AuthApp.Roles.Dto;
using AuthApp.Users.Dto;
using AutoMapper;
namespace AuthApp.Identity.Dto
{
    public class IdentityMapProfile : Profile
    {
        public IdentityMapProfile()
        {
            CreateMap<UserOutputDto, User>();
            CreateMap<UserInputDto, User>();
            CreateMap<RoleInputDto, Role>();
        }
    }
}
