using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.BLL.Models.Geocoding;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class GeolocationsMappingProfile : Profile
{
    public GeolocationsMappingProfile()
    {
        CreateMap<ReverseGeocodingResult, Geolocation>()
            .ForMember(geo => geo.Id, o => o.Ignore())
            .ForMember(geo => geo.Latitude, o => o.MapFrom(res => double.Parse(res.Latitude)))
            .ForMember(geo => geo.Longitude, o => o.MapFrom(res => double.Parse(res.Longitude)))
            .ForMember(geo => geo.Country, o => o.MapFrom(res => res.Address.Country))
            .ForMember(geo => geo.City, o => o.MapFrom(res => string.IsNullOrEmpty(res.Address.City) ? res.Address.Town : res.Address.City))
            .ForMember(geo => geo.PostalCode, o => o.MapFrom(res => res.Address.Postcode))
            .ForMember(geo => geo.Address, o => o.MapFrom(res => res.DisplayAddress));

        CreateMap<Geolocation, GeolocationDTO>().ReverseMap();

        CreateMap<GeolocationDTO, GeolocationResponse>();
    }
}
