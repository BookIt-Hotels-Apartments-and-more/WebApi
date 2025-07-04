using AutoMapper;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class ImagesMappingProfile : Profile
{
    public ImagesMappingProfile()
    {
        CreateMap<Image, ImageDTO>();

        CreateMap<ImageDTO, ImageResponse>();
    }
}