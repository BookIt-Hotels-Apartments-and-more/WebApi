using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class FavoritesMappingProfile : Profile
{
    public FavoritesMappingProfile()
    {
        CreateMap<FavoriteRequest, FavoriteDTO>();

        CreateMap<FavoriteDTO, Favorite>()
            .ForMember(a => a.Id, o => o.Ignore());

        CreateMap<Favorite, FavoriteDTO>();

        CreateMap<FavoriteDTO, FavoriteResponse>();
    }
}
