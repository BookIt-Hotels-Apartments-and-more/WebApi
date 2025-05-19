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

        CreateMap<ApartmentDTO, ApartmentResponse>();
    }
}
