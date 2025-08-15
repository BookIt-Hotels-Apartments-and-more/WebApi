using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class RatingsMappingProfile : Profile
{
    public RatingsMappingProfile()
    {
        CreateMap<ApartmentRating, ApartmentRatingDTO>().ReverseMap();

        CreateMap<ApartmentRatingDTO, ApartmentRatingResponse>().ReverseMap();

        CreateMap<ApartmentRating, ApartmentRatingResponse>();

        CreateMap<UserRating, UserRatingDTO>().ReverseMap();

        CreateMap<UserRatingDTO, UserRatingResponse>().ReverseMap();

        CreateMap<UserRating, UserRatingResponse>();
    }
}