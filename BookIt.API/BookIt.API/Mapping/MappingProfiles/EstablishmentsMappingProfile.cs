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
        CreateMap<EstablishmentRequest, EstablishmentDTO>()
            .ForMember(dto => dto.Photos,
                       o => o.MapFrom(req => req.ExistingPhotosIds.Select(id => new ImageDTO { Id = id })
                                             .Union(req.NewPhotosBase64.Select(base64 => new ImageDTO { Base64Image = base64 }))));

        CreateMap<EstablishmentDTO, Establishment>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.Photos, o => o.Ignore())
            .ForMember(e => e.CreatedAt, o => o.Ignore());

        CreateMap<Establishment, EstablishmentDTO>();

        CreateMap<EstablishmentDTO, EstablishmentResponse>();

        CreateMap<User, OwnerDTO>();

        CreateMap<OwnerDTO, OwnerResponse>();
    }
}
