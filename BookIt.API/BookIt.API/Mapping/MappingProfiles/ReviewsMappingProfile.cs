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
            .ForMember(a => a.Id, o => o.Ignore())
            .ForMember(a => a.Photos, o => o.Ignore())
            .ForMember(a => a.CreatedAt, o => o.Ignore());

        CreateMap<Review, ReviewDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(a => a.Photos.Select(im => im.BlobUrl)));

        CreateMap<ReviewDTO, ReviewResponse>();
    }
}
