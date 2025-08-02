using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Mapping.MappingProfiles;

public class BookingsMappingProfile : Profile
{
    public BookingsMappingProfile()
    {
        CreateMap<BookingRequest, BookingDTO>();

        CreateMap<Booking, BookingDTO>()
            .ForMember(dto => dto.Customer, o => o.MapFrom(b => b.User))
            .ForMember(dto => dto.CustomerId, o => o.MapFrom(b => b.UserId));

        CreateMap<BookingDTO, Booking>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.CreatedAt, o => o.Ignore())
            .ForMember(e => e.IsCheckedIn, o => o.Ignore())
            .ForMember(e => e.UserId, o => o.MapFrom(dto => dto.CustomerId));

        CreateMap<BookingDTO, BookingResponse>();

        CreateMap<User, CustomerDTO>()
            .ForMember(dto => dto.Photos, o => o.MapFrom(u => u.Photos.Select(im => im.BlobUrl)))
            .ForMember(dto => dto.Rating, o => o.MapFrom(u => u.Rating));

        CreateMap<CustomerDTO, CustomerResponse>()
            .ForMember(res => res.Rating, o => o.MapFrom(dto => dto.Rating));
    }
}
