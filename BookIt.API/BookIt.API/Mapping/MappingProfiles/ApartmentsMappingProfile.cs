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
        CreateMap<ApartmentRequest, ApartmentDTO>()
            .ForMember(dto => dto.Photos,
                       o => o.MapFrom(req => req.ExistingPhotosIds.Select(id => new ImageDTO { Id = id })
                                             .Union(req.NewPhotosBase64.Select(base64 => new ImageDTO { Base64Image = base64 }))));

        CreateMap<ApartmentDTO, Apartment>()
            .ForMember(a => a.Id, o => o.Ignore())
            .ForMember(a => a.Photos, o => o.Ignore())
            .ForMember(a => a.CreatedAt, o => o.Ignore());

        CreateMap<Apartment, ApartmentDTO>();

        CreateMap<ApartmentDTO, ApartmentResponse>();
    }
}
