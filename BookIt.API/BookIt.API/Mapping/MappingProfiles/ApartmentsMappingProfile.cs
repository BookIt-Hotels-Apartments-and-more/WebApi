using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class ApartmentsMappingProfile : Profile
{
    public ApartmentsMappingProfile()
    {
        CreateMap<ApartmentRequest, ApartmentDTO>()
            .ForMember(dto => dto.Photos,
                       o => o.MapFrom(req => req.ExistingPhotosIds.Select(id => new ImageDTO { Id = id })
                                             .Union(req.NewPhotosBase64.Select(base64 => new ImageDTO { Base64Image = base64 }))));

        CreateMap<Apartment, ApartmentDTO>()
            .ForMember(dto => dto.Rating, opt => opt.MapFrom(src => src.Rating));

        CreateMap<ApartmentDTO, Apartment>()
            .ForMember(a => a.Id, o => o.Ignore())
            .ForMember(a => a.Photos, o => o.Ignore())
            .ForMember(a => a.CreatedAt, o => o.Ignore())
            .ForMember(a => a.Rating, o => o.Ignore());

        CreateMap<ApartmentDTO, ApartmentResponse>()
            .ForMember(res => res.Rating, opt => opt.MapFrom(dto => dto.Rating))
            .ForMember(res => res.Features, opt => opt.MapFrom(dto => new ApartmentFeaturesResponse
            {
                FreeWifi = (dto.Features & ApartmentFeatures.FreeWifi) != 0,
                AirConditioning = (dto.Features & ApartmentFeatures.AirConditioning) != 0,
                Breakfast = (dto.Features & ApartmentFeatures.Breakfast) != 0,
                Kitchen = (dto.Features & ApartmentFeatures.Kitchen) != 0,
                TV = (dto.Features & ApartmentFeatures.TV) != 0,
                Balcony = (dto.Features & ApartmentFeatures.Balcony) != 0,
                Bathroom = (dto.Features & ApartmentFeatures.Bathroom) != 0,
                PetsAllowed = (dto.Features & ApartmentFeatures.PetsAllowed) != 0
            }));

        CreateMap<User, OwnerDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(u => u.Photos.Select(im => im.BlobUrl)));

        CreateMap<OwnerDTO, OwnerResponse>();
    }
}