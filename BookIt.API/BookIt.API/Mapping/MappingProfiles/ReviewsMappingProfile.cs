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
        CreateMap<ReviewRequest, ReviewDTO>();

        CreateMap<ReviewDTO, Review>()
            .ForMember(r => r.Id, o => o.Ignore())
            .ForMember(r => r.Photos, o => o.Ignore())
            .ForMember(r => r.CreatedAt, o => o.Ignore())
            .ForMember(r => r.UserId, o => o.MapFrom(dto => dto.CustomerId));

        CreateMap<Review, ReviewDTO>()
            .ForMember(dto => dto.CustomerId, o => o.MapFrom(r => r.UserId))
            .ForMember(dto => dto.Customer, o => o.MapFrom(r => r.User))
            .ForMember(dto => dto.Photos, o => o.MapFrom(r => r.Photos.Select(im => im.BlobUrl)));

        CreateMap<ReviewDTO, ReviewResponse>();
    }
}
