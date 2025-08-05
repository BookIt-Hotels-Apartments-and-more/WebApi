using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class BookingsService : IBookingsService
{
    private readonly IMapper _mapper;
    private readonly BookingsRepository _bookingsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;

    public BookingsService(
        IMapper mapper,
        BookingsRepository bookingsRepository,
        ApartmentsRepository apartmentsRepository)
    {
        _mapper = mapper;
        _bookingsRepository = bookingsRepository;
        _apartmentsRepository = apartmentsRepository;
    }

    public async Task<IEnumerable<BookingDTO>> GetAllAsync()
    {
        var bookingsDomain = await _bookingsRepository.GetAllAsync();
        var bookingsDto = _mapper.Map<IEnumerable<BookingDTO>>(bookingsDomain);
        return bookingsDto;
    }

    public async Task<BookingDTO?> GetByIdAsync(int id)
    {
        var bookingDomain = await _bookingsRepository.GetByIdAsync(id);
        if (bookingDomain is null) return null;
        var apartmentsDto = _mapper.Map<BookingDTO>(bookingDomain);
        return apartmentsDto;
    }

    public async Task<BookingDTO?> CreateAsync(BookingDTO dto)
    {
        var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId);
        if (apartment is null) throw new ArgumentException($"Apartment with ID {dto.ApartmentId} does not exist.");

        dto.DateFrom = ConfigureDateTimeForBooking(dto.DateFrom, apartment.Establishment.CheckInTime);
        dto.DateTo = ConfigureDateTimeForBooking(dto.DateTo, apartment.Establishment.CheckOutTime);

        if (dto.DateFrom >= dto.DateTo) throw new ArgumentException("Check-out date must be after check-in date");
        if (dto.DateFrom.Date < DateTime.UtcNow.Date) throw new ArgumentException("Cannot create bookings for past dates");

        var isAvailable = await _bookingsRepository.IsApartmentAvailableAsync(
            dto.ApartmentId,
            dto.DateFrom,
            dto.DateTo
        );

        if (!isAvailable)
        {
            var conflictingBookings = await _bookingsRepository.GetConflictingBookingsAsync(
                dto.ApartmentId,
                dto.DateFrom,
                dto.DateTo
            );

            var conflictDetails = conflictingBookings
                .Select(b => $"Booking #{b.Id}: {b.DateFrom:yyyy-MM-dd} to {b.DateTo:yyyy-MM-dd} (User: {b.User.Username})")
                .ToList();

            throw new BookingConflictException(dto.ApartmentId, dto.DateFrom, dto.DateTo, conflictDetails);
        }

        var bookingDomain = _mapper.Map<Booking>(dto);
        var addedBooking = await _bookingsRepository.AddAsync(bookingDomain);
        return await GetByIdAsync(addedBooking.Id);
    }

    public async Task<BookingDTO?> UpdateAsync(int id, BookingDTO dto)
    {
        var bookingExists = await _bookingsRepository.ExistsAsync(id);
        if (!bookingExists) return null;

        var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId);
        if (apartment is null) throw new ArgumentException($"Apartment with ID {dto.ApartmentId} does not exist.");

        dto.DateFrom = ConfigureDateTimeForBooking(dto.DateFrom, apartment.Establishment.CheckInTime);
        dto.DateTo = ConfigureDateTimeForBooking(dto.DateTo, apartment.Establishment.CheckOutTime);

        if (dto.DateFrom >= dto.DateTo) throw new ArgumentException("Check-out date must be after check-in date");

        var isAvailable = await _bookingsRepository.IsApartmentAvailableAsync(
            dto.ApartmentId,
            dto.DateFrom,
            dto.DateTo,
            id
        );

        if (!isAvailable)
        {
            var conflictingBookings = await _bookingsRepository.GetConflictingBookingsAsync(
                dto.ApartmentId,
                dto.DateFrom,
                dto.DateTo,
                id
            );

            var conflictDetails = conflictingBookings
                .Select(b => $"Booking #{b.Id}: {b.DateFrom:yyyy-MM-dd} to {b.DateTo:yyyy-MM-dd} (User: {b.User.Username})")
                .ToList();

            throw new BookingConflictException(dto.ApartmentId, dto.DateFrom, dto.DateTo, conflictDetails);
        }

        var bookingDomain = _mapper.Map<Booking>(dto);
        bookingDomain.Id = id;
        await _bookingsRepository.UpdateAsync(bookingDomain);
        return await GetByIdAsync(id);
    }

    public async Task<bool> CheckAvailabilityAsync(int apartmentId, DateTime dateFrom, DateTime dateTo)
    {
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment is null) throw new ArgumentException($"Apartment with ID {apartmentId} does not exist.");

        var adjustedDateFrom = ConfigureDateTimeForBooking(dateFrom, apartment.Establishment.CheckInTime);
        var adjustedDateTo = ConfigureDateTimeForBooking(dateTo, apartment.Establishment.CheckOutTime);

        return await _bookingsRepository.IsApartmentAvailableAsync(apartmentId, adjustedDateFrom, adjustedDateTo);
    }

    public async Task<List<(DateTime DateFrom, DateTime DateTo)>> GetBookedDatesAsync(int apartmentId)
    {
        return await _bookingsRepository.GetBookedDatesAsync(apartmentId);
    }

    public async Task<BookingDTO?> CheckInAsync(int id)
    {
        var apartmentExists = await _bookingsRepository.ExistsAsync(id);
        if (!apartmentExists) return null;
        var bookingDomain = await _bookingsRepository.CheckInAsync(id);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var apartmentExists = await _bookingsRepository.ExistsAsync(id);
        if (!apartmentExists) return false;
        await _bookingsRepository.DeleteAsync(id);
        return true;
    }

    public async Task<ApartmentAvailabilityDTO> GetApartmentAvailabilityAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment is null) throw new ArgumentException($"Apartment with ID {apartmentId} does not exist.");

        DateTime? adjustedStartDate = startDate.HasValue
            ? ConfigureDateTimeForBooking(startDate.Value, apartment.Establishment.CheckInTime)
            : null;
        DateTime? adjustedEndDate = endDate.HasValue
            ? ConfigureDateTimeForBooking(endDate.Value, apartment.Establishment.CheckOutTime)
            : null;

        var bookedDays = await _bookingsRepository.GetBookedDaysAsync(apartmentId, adjustedStartDate, adjustedEndDate);
        var bookedRanges = await _bookingsRepository.GetBookedDateRangesAsync(apartmentId, adjustedStartDate, adjustedEndDate);

        return new ApartmentAvailabilityDTO
        {
            ApartmentId = apartmentId,
            UnavailableDates = bookedDays,
            BookedPeriods = bookedRanges.Select(r => new BookedPeriod
            {
                StartDate = r.DateFrom,
                EndDate = r.DateTo,
                BookingId = r.BookingId
            }).ToList()
        };
    }

    private DateTime ConfigureDateTimeForBooking(DateTime providedDateTime, TimeOnly timeToSet)
    {
        var dateOnly = DateOnly.FromDateTime(providedDateTime);
        return new DateTime(dateOnly, timeToSet);
    }
}
