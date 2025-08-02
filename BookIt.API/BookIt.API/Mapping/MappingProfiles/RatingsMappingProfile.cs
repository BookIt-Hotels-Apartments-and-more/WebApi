using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class RatingsMappingProfile : Profile
{
    public RatingsMappingProfile()
    {
        CreateMap<Rating, RatingDTO>().ReverseMap();

        CreateMap<RatingDTO, RatingResponse>().ReverseMap();

        CreateMap<Rating, RatingResponse>();
    }
}