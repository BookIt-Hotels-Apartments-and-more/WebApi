using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class EstablishmentsMappingProfile : Profile
{
    public EstablishmentsMappingProfile()
    {
        CreateMap<EstablishmentFilterRequest, EstablishmentFilterDTO>();

        CreateMap<EstablishmentRequest, EstablishmentDTO>()
            .ForMember(dto => dto.Geolocation,
                       o => o.MapFrom(req => new GeolocationDTO { Latitude = req.Latitude, Longitude = req.Longitude }))
            .ForMember(dto => dto.Photos,
                       o => o.MapFrom(req => req.ExistingPhotosIds.Select(id => new ImageDTO { Id = id })
                                             .Union(req.NewPhotosBase64.Select(base64 => new ImageDTO { Base64Image = base64 }))));

        CreateMap<Establishment, EstablishmentDTO>()
            .ForMember(dto => dto.Owner, o => o.MapFrom(e => e.Owner))
            .ForMember(dto => dto.Rating, opt => opt.MapFrom(src => src.Rating));

        CreateMap<EstablishmentDTO, Establishment>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.Vibe, o => o.Ignore())
            .ForMember(e => e.Photos, o => o.Ignore())
            .ForMember(e => e.CreatedAt, o => o.Ignore())
            .ForMember(e => e.Geolocation, o => o.Ignore())
            .ForMember(e => e.Rating, o => o.Ignore());

        CreateMap<EstablishmentDTO, EstablishmentResponse>()
            .ForMember(res => res.Rating, opt => opt.MapFrom(dto => dto.Rating))
            .ForMember(res => res.Features, opt => opt.MapFrom(dto => new EstablishmentFeaturesResponse
            {
                Parking = (dto.Features & EstablishmentFeatures.Parking) != 0,
                Pool = (dto.Features & EstablishmentFeatures.Pool) != 0,
                Beach = (dto.Features & EstablishmentFeatures.Beach) != 0,
                Fishing = (dto.Features & EstablishmentFeatures.Fishing) != 0,
                Sauna = (dto.Features & EstablishmentFeatures.Sauna) != 0,
                Restaurant = (dto.Features & EstablishmentFeatures.Restaurant) != 0,
                Smoking = (dto.Features & EstablishmentFeatures.Smoking) != 0,
                AccessibleForDisabled = (dto.Features & EstablishmentFeatures.AccessibleForDisabled) != 0,
                ElectricCarCharging = (dto.Features & EstablishmentFeatures.ElectricCarCharging) != 0,
                Elevator = (dto.Features & EstablishmentFeatures.Elevator) != 0
            }));
    }
}
