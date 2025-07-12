using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.BLL.DTOs;

namespace BookIt.API.MappingProfiles;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // ...existing mappings...
        
        CreateMap<EstablishmentFilterRequest, EstablishmentFilterDTO>();
    }
}
