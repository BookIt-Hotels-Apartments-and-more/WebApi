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

        CreateMap<Establishment, EstablishmentDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(e => e.Photos.Select(im => new ImageDTO { BlobUrl = im.BlobUrl })))
            .ForMember(dto => dto.Owner, o => o.MapFrom(e => e.Owner));

        CreateMap<EstablishmentDTO, EstablishmentResponse>();

        CreateMap<User, OwnerDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(u => u.Photos.Select(im => im.BlobUrl)));

        CreateMap<OwnerDTO, OwnerResponse>();

        CreateMap<EstablishmentDTO, EstablishmentResponse>()
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => new EstablishmentFeaturesResponse
            {
                Parking = (src.Features & EstablishmentFeatures.Parking) != 0,
                Pool = (src.Features & EstablishmentFeatures.Pool) != 0,
                Beach = (src.Features & EstablishmentFeatures.Beach) != 0,
                Fishing = (src.Features & EstablishmentFeatures.Fishing) != 0,
                Sauna = (src.Features & EstablishmentFeatures.Sauna) != 0,
                Restaurant = (src.Features & EstablishmentFeatures.Restaurant) != 0,
                Smoking = (src.Features & EstablishmentFeatures.Smoking) != 0,
                AccessibleForDisabled = (src.Features & EstablishmentFeatures.AccessibleForDisabled) != 0,
                ElectricCarCharging = (src.Features & EstablishmentFeatures.ElectricCarCharging) != 0,
                Elevator = (src.Features & EstablishmentFeatures.Elevator) != 0
            }))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
}
