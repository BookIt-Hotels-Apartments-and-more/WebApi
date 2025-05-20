using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class EstablishmentsMappingProfile : Profile
{
    public EstablishmentsMappingProfile()
    {
        CreateMap<EstablishmentRequest, EstablishmentDTO>();

        CreateMap<EstablishmentDTO, Establishment>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.Photos, o => o.Ignore())
            .ForMember(e => e.CreatedAt, o => o.Ignore());

        CreateMap<Establishment, EstablishmentDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(e => e.Photos.Select(im => im.BlobUrl)));

        CreateMap<EstablishmentDTO, EstablishmentResponse>();

        CreateMap<User, OwnerDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(u => u.Photos.Select(im => im.BlobUrl)));

        CreateMap<OwnerDTO, OwnerResponse>();
    }
}
