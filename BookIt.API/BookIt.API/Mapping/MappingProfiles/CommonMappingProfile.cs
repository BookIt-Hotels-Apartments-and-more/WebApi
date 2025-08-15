using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;

namespace BookIt.API.Mapping.MappingProfiles;

public class CommonMappingProfile : Profile
{
    public CommonMappingProfile()
    {
        CreateMap<PagedResultDTO<ReviewDTO>, PaginatedResponse<ReviewResponse>>();

        CreateMap<PagedResultDTO<ApartmentDTO>, PaginatedResponse<ApartmentResponse>>();

        CreateMap<PagedResultDTO<EstablishmentDTO>, PaginatedResponse<EstablishmentResponse>>();
    }
}