using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class BookingsService : IBookingsService
{
    private readonly IMapper _mapper;
    private readonly BookingsRepository _repository;

    public BookingsService(IMapper mapper, BookingsRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<IEnumerable<BookingDTO>> GetAllAsync()
    {
        var bookingsDomain = await _repository.GetAllAsync();
        var bookingsDto = _mapper.Map<IEnumerable<BookingDTO>>(bookingsDomain);
        return bookingsDto;
    }

    public async Task<BookingDTO?> GetByIdAsync(int id)
    {
        var bookingDomain = await _repository.GetByIdAsync(id);
        if (bookingDomain is null) return null;
        var apartmentsDto = _mapper.Map<BookingDTO>(bookingDomain);
        return apartmentsDto;
    }

    public async Task<BookingDTO?> CreateAsync(BookingDTO dto)
    {
        var bookingDomain = _mapper.Map<Booking>(dto);
        var addedBooking = await _repository.AddAsync(bookingDomain);
        return await GetByIdAsync(addedBooking.Id);
    }

    public async Task<BookingDTO?> UpdateAsync(int id, BookingDTO dto)
    {
        var apartmentExists = await _repository.ExistsAsync(id);
        if (!apartmentExists) return null;
        var bookingDomain = _mapper.Map<Booking>(dto);
        bookingDomain.Id = id;
        await _repository.UpdateAsync(bookingDomain);
        return await GetByIdAsync(id);
    }

    public async Task<BookingDTO?> CheckInAsync(int id)
    {
        var apartmentExists = await _repository.ExistsAsync(id);
        if (!apartmentExists) return null;
        var bookingDomain = await _repository.CheckInAsync(id);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var apartmentExists = await _repository.ExistsAsync(id);
        if (!apartmentExists) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
