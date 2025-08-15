using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDTO>();

        CreateMap<UserDTO, UserResponse>();

        CreateMap<UserDTO, UserAuthResponse>()
            .ForMember(res => res.Role, opt => opt.MapFrom(dto => (int)dto.Role));

        CreateMap<UserDetailsRequest, UserDetailsDTO>();
    }
}
