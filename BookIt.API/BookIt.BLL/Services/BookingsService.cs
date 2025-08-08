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
        try
        {
            var bookingsDomain = await _bookingsRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BookingDTO>>(bookingsDomain);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve bookings", ex);
        }
    }

    public async Task<BookingDTO?> GetByIdAsync(int id)
    {
        try
        {
            var bookingDomain = await _bookingsRepository.GetByIdAsync(id);
            if (bookingDomain is null)
            {
                throw new EntityNotFoundException("Booking", id);
            }

            return _mapper.Map<BookingDTO>(bookingDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve booking", ex);
        }
    }

    public async Task<BookingDTO?> CreateAsync(BookingDTO dto)
    {
        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId);
            if (apartment is null)
            {
                throw new EntityNotFoundException("Apartment", dto.ApartmentId);
            }

            dto.DateFrom = ConfigureDateTimeForBooking(dto.DateFrom, apartment.Establishment.CheckInTime);
            dto.DateTo = ConfigureDateTimeForBooking(dto.DateTo, apartment.Establishment.CheckOutTime);

            ValidateBookingDates(dto.DateFrom, dto.DateTo);

            await ValidateBookingAvailabilityAsync(dto.ApartmentId, dto.DateFrom, dto.DateTo);

            var bookingDomain = _mapper.Map<Booking>(dto);
            var addedBooking = await _bookingsRepository.AddAsync(bookingDomain);
            return await GetByIdAsync(addedBooking.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to create booking", ex);
        }
    }

    public async Task<BookingDTO?> UpdateAsync(int id, BookingDTO dto)
    {
        try
        {
            var bookingExists = await _bookingsRepository.ExistsAsync(id);
            if (!bookingExists)
            {
                throw new EntityNotFoundException("Booking", id);
            }

            var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId);
            if (apartment is null)
            {
                throw new EntityNotFoundException("Apartment", dto.ApartmentId);
            }

            dto.DateFrom = ConfigureDateTimeForBooking(dto.DateFrom, apartment.Establishment.CheckInTime);
            dto.DateTo = ConfigureDateTimeForBooking(dto.DateTo, apartment.Establishment.CheckOutTime);

            ValidateBookingDates(dto.DateFrom, dto.DateTo);

            await ValidateBookingAvailabilityAsync(dto.ApartmentId, dto.DateFrom, dto.DateTo, id);

            var bookingDomain = _mapper.Map<Booking>(dto);
            bookingDomain.Id = id;
            await _bookingsRepository.UpdateAsync(bookingDomain);

            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update booking", ex);
        }
    }

    public async Task<BookingDTO?> CheckInAsync(int id)
    {
        try
        {
            var bookingExists = await _bookingsRepository.ExistsAsync(id);
            if (!bookingExists)
            {
                throw new EntityNotFoundException("Booking", id);
            }

            var booking = await _bookingsRepository.GetByIdAsync(id);
            if (booking is null)
            {
                throw new EntityNotFoundException("Booking", id);
            }

            if (DateTime.UtcNow.Date < booking.DateFrom.Date)
            {
                throw new BusinessRuleViolationException(
                    "EARLY_CHECKIN_NOT_ALLOWED",
                    $"Cannot check in before booking start date: {booking.DateFrom:yyyy-MM-dd}");
            }

            if (booking.IsCheckedIn)
            {
                throw new BusinessRuleViolationException(
                    "ALREADY_CHECKED_IN",
                    "Booking has already been checked in");
            }

            if (DateTime.UtcNow.Date > booking.DateTo.Date)
            {
                throw new BusinessRuleViolationException(
                    "BOOKING_EXPIRED",
                    $"Cannot check in after booking end date: {booking.DateTo:yyyy-MM-dd}");
            }

            await _bookingsRepository.CheckInAsync(id);
            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to check in booking", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var bookingExists = await _bookingsRepository.ExistsAsync(id);
            if (!bookingExists)
            {
                throw new EntityNotFoundException("Booking", id);
            }

            var booking = await _bookingsRepository.GetByIdAsync(id);
            if (booking is null)
            {
                throw new EntityNotFoundException("Booking", id);
            }

            if (DateTime.UtcNow.Date >= booking.DateFrom.Date)
            {
                throw new BusinessRuleViolationException(
                    "CANNOT_DELETE_ACTIVE_BOOKING",
                    "Cannot delete a booking that has already started or is in progress");
            }

            if (booking.IsCheckedIn)
            {
                throw new BusinessRuleViolationException(
                    "CANNOT_DELETE_CHECKED_IN_BOOKING",
                    "Cannot delete a booking that has been checked in");
            }

            await _bookingsRepository.DeleteAsync(id);
            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to delete booking", ex);
        }
    }

    public async Task<bool> CheckAvailabilityAsync(int apartmentId, DateTime dateFrom, DateTime dateTo)
    {
        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            var adjustedDateFrom = ConfigureDateTimeForBooking(dateFrom, apartment.Establishment.CheckInTime);
            var adjustedDateTo = ConfigureDateTimeForBooking(dateTo, apartment.Establishment.CheckOutTime);

            ValidateBookingDates(adjustedDateFrom, adjustedDateTo);

            return await _bookingsRepository.IsApartmentAvailableAsync(apartmentId, adjustedDateFrom, adjustedDateTo);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to check apartment availability", ex);
        }
    }

    public async Task<List<(DateTime DateFrom, DateTime DateTo)>> GetBookedDatesAsync(int apartmentId)
    {
        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            return await _bookingsRepository.GetBookedDatesAsync(apartmentId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve booked dates", ex);
        }
    }

    public async Task<ApartmentAvailabilityDTO> GetApartmentAvailabilityAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            DateTime? adjustedStartDate = startDate.HasValue
                ? ConfigureDateTimeForBooking(startDate.Value, apartment.Establishment.CheckInTime)
                : null;
            DateTime? adjustedEndDate = endDate.HasValue
                ? ConfigureDateTimeForBooking(endDate.Value, apartment.Establishment.CheckOutTime)
                : null;

            if (adjustedStartDate.HasValue && adjustedEndDate.HasValue)
            {
                ValidateBookingDates(adjustedStartDate.Value, adjustedEndDate.Value);
            }

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
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve apartment availability", ex);
        }
    }

    private DateTime ConfigureDateTimeForBooking(DateTime providedDateTime, TimeOnly timeToSet)
    {
        try
        {
            var dateOnly = DateOnly.FromDateTime(providedDateTime);
            return new DateTime(dateOnly, timeToSet);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("DateTime", "Failed to configure booking datetime", ex);
        }
    }

    private void ValidateBookingDates(DateTime dateFrom, DateTime dateTo)
    {
        if (dateFrom >= dateTo)
        {
            throw new BusinessRuleViolationException(
                "INVALID_DATE_RANGE",
                "Check-out date must be after check-in date");
        }

        if (dateFrom.Date < DateTime.UtcNow.Date)
        {
            throw new BusinessRuleViolationException(
                "PAST_DATE_BOOKING",
                "Cannot create bookings for past dates");
        }

        var maxBookingDays = 30;
        if ((dateTo.Date - dateFrom.Date).TotalDays > maxBookingDays)
        {
            throw new BusinessRuleViolationException(
                "BOOKING_TOO_LONG",
                $"Booking duration cannot exceed {maxBookingDays} days");
        }

        if ((dateFrom - DateTime.UtcNow).TotalHours < 1)
        {
            throw new BusinessRuleViolationException(
                "INSUFFICIENT_ADVANCE_BOOKING",
                "Booking must be made at least 1 hour in advance");
        }
    }

    private async Task ValidateBookingAvailabilityAsync(int apartmentId, DateTime dateFrom, DateTime dateTo, int? excludeBookingId = null)
    {
        var isAvailable = await _bookingsRepository.IsApartmentAvailableAsync(
            apartmentId, dateFrom, dateTo, excludeBookingId);

        if (!isAvailable)
        {
            var conflictingBookings = await _bookingsRepository.GetConflictingBookingsAsync(
                apartmentId, dateFrom, dateTo, excludeBookingId);

            var conflictDetails = conflictingBookings
                .Select(b => $"Booking #{b.Id}: {b.DateFrom:yyyy-MM-dd} to {b.DateTo:yyyy-MM-dd} (User: {b.User.Username})")
                .ToList();

            throw new BookingConflictException(apartmentId, dateFrom, dateTo, conflictDetails);
        }
    }
}