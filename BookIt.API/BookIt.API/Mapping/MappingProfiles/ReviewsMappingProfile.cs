using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class ReviewsMappingProfile : Profile
{
    public ReviewsMappingProfile()
    {
        CreateMap<ReviewFilterRequest, ReviewFilterDTO>();

        CreateMap<ReviewRequest, ReviewDTO>()
            .ForMember(dto => dto.Photos,
                       o => o.MapFrom(req => req.ExistingPhotosIds.Select(id => new ImageDTO { Id = id })
                                             .Union(req.NewPhotosBase64.Select(base64 => new ImageDTO { Base64Image = base64 }))))
            .ForMember(dto => dto.Rating, opt => opt.MapFrom(src => CalculateOverallRating(src)));

        CreateMap<Review, ReviewDTO>()
            .ForMember(dto => dto.CustomerId, o => o.MapFrom(r => r.UserId))
            .ForMember(dto => dto.Customer, o => o.MapFrom(r => r.User));

        CreateMap<ReviewDTO, Review>()
            .ForMember(r => r.Id, o => o.Ignore())
            .ForMember(r => r.Photos, o => o.Ignore())
            .ForMember(r => r.CreatedAt, o => o.Ignore())
            .ForMember(r => r.UserId, o => o.MapFrom(dto => dto.CustomerId));

        CreateMap<ReviewDTO, ReviewResponse>();
    }

    private static float CalculateOverallRating(ReviewRequest src)
    {
        if (src.ApartmentId.HasValue)
        {
            var apartmentRatings = new[] {
                src.StaffRating!.Value, src.PurityRating!.Value, src.PriceQualityRating!.Value,
                src.ComfortRating!.Value, src.FacilitiesRating!.Value, src.LocationRating!.Value
            };
            return apartmentRatings.Average();
        }

        return src.CustomerStayRating!.Value;
    }
}