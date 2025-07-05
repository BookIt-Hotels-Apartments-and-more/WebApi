using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class ApartmentsMappingProfile : Profile
{
    public ApartmentsMappingProfile()
    {
        CreateMap<ApartmentRequest, ApartmentDTO>();

        CreateMap<ApartmentDTO, Apartment>()
            .ForMember(a => a.Id, o => o.Ignore())
            .ForMember(a => a.Photos, o => o.Ignore())
            .ForMember(a => a.CreatedAt, o => o.Ignore());

        CreateMap<Apartment, ApartmentDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(a => a.Photos.Select(im => im.BlobUrl)));

        CreateMap<ApartmentDTO, ApartmentResponse>()
        .ForMember(dest => dest.Features, opt => opt.MapFrom(src => new ApartmentFeaturesResponse
        {
            FreeWifi = (src.Features & ApartmentFeatures.FreeWifi) != 0,
            AirConditioning = (src.Features & ApartmentFeatures.AirConditioning) != 0,
            Breakfast = (src.Features & ApartmentFeatures.Breakfast) != 0,
            Kitchen = (src.Features & ApartmentFeatures.Kitchen) != 0,
            TV = (src.Features & ApartmentFeatures.TV) != 0,
            Balcony = (src.Features & ApartmentFeatures.Balcony) != 0,
            Bathroom = (src.Features & ApartmentFeatures.Bathroom) != 0,
            PetsAllowed = (src.Features & ApartmentFeatures.PetsAllowed) != 0
        }));
    }
}
