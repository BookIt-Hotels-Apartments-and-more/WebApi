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

        CreateMap<User, UserAuthDTO>();

        CreateMap<UserDTO, UserResponse>()
            .ForMember(res => res.Reviews, opt => opt.Ignore());

        CreateMap<UserAuthDTO, UserAuthResponse>()
            .ForMember(res => res.Role, opt => opt.MapFrom(dto => (int)dto.Role));

        CreateMap<UserDetailsRequest, UserDetailsDTO>();
    }
}
