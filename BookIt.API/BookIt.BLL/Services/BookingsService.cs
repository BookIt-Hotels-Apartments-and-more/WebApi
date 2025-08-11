using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BookIt.BLL.Services;

public class BookingsService : IBookingsService
{
    private readonly IMapper _mapper;
    private readonly ILogger<BookingsService> _logger;
    private readonly BookingsRepository _bookingsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;

    public BookingsService(
        IMapper mapper,
        ILogger<BookingsService> logger,
        BookingsRepository bookingsRepository,
        ApartmentsRepository apartmentsRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bookingsRepository = bookingsRepository ?? throw new ArgumentNullException(nameof(bookingsRepository));
        _apartmentsRepository = apartmentsRepository ?? throw new ArgumentNullException(nameof(apartmentsRepository));
    }

    public async Task<IEnumerable<BookingDTO>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all bookings");
        try
        {
            var bookingsDomain = await _bookingsRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} bookings", bookingsDomain.Count());
            return _mapper.Map<IEnumerable<BookingDTO>>(bookingsDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all bookings");
            throw new ExternalServiceException("Database", "Failed to retrieve bookings", ex);
        }
    }

    public async Task<BookingDTO?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving booking with ID {BookingId}", id);
        try
        {
            var bookingDomain = await _bookingsRepository.GetByIdAsync(id);
            if (bookingDomain is null)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found", id);
                throw new EntityNotFoundException("Booking", id);
            }

            _logger.LogInformation("Booking with ID {BookingId} retrieved successfully", id);
            return _mapper.Map<BookingDTO>(bookingDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve booking with ID {BookingId}", id);
            throw new ExternalServiceException("Database", "Failed to retrieve booking", ex);
        }
    }

    public async Task<BookingDTO?> CreateAsync(BookingDTO dto)
    {
        _logger.LogInformation("Creating booking for Apartment ID {ApartmentId} from {DateFrom} to {DateTo}",
            dto.ApartmentId, dto.DateFrom, dto.DateTo);

        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId);
            if (apartment is null)
            {
                _logger.LogWarning("Apartment with ID {ApartmentId} not found", dto.ApartmentId);
                throw new EntityNotFoundException("Apartment", dto.ApartmentId);
            }

            dto.DateFrom = ConfigureDateTimeForBooking(dto.DateFrom, apartment.Establishment.CheckInTime);
            dto.DateTo = ConfigureDateTimeForBooking(dto.DateTo, apartment.Establishment.CheckOutTime);

            ValidateBookingDates(dto.DateFrom, dto.DateTo);
            await ValidateBookingAvailabilityAsync(dto.ApartmentId, dto.DateFrom, dto.DateTo);

            var bookingDomain = _mapper.Map<Booking>(dto);
            var addedBooking = await _bookingsRepository.AddAsync(bookingDomain);
            _logger.LogInformation("Booking created with ID {BookingId}", addedBooking.Id);

            return await GetByIdAsync(addedBooking.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create booking for Apartment ID {ApartmentId}", dto.ApartmentId);
            throw new ExternalServiceException("Database", "Failed to create booking", ex);
        }
    }

    public async Task<BookingDTO?> UpdateAsync(int id, BookingDTO dto)
    {
        _logger.LogInformation("Updating booking with ID {BookingId}", id);
        try
        {
            var bookingExists = await _bookingsRepository.ExistsAsync(id);
            if (!bookingExists)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found", id);
                throw new EntityNotFoundException("Booking", id);
            }

            var checkInAndCheckOutTimes = await _apartmentsRepository.GetCheckInAndCheckOutTimeForApartment(dto.ApartmentId);
            if (checkInAndCheckOutTimes is null)
            {
                _logger.LogWarning("Apartment with ID {ApartmentId} not found", dto.ApartmentId);
                throw new EntityNotFoundException("Apartment", dto.ApartmentId);
            }

            dto.DateFrom = ConfigureDateTimeForBooking(dto.DateFrom, checkInAndCheckOutTimes.Value.CheckInTime);
            dto.DateTo = ConfigureDateTimeForBooking(dto.DateTo, checkInAndCheckOutTimes.Value.CheckOutTime);

            ValidateBookingDates(dto.DateFrom, dto.DateTo);
            await ValidateBookingAvailabilityAsync(dto.ApartmentId, dto.DateFrom, dto.DateTo, id);

            var bookingDomain = _mapper.Map<Booking>(dto);
            bookingDomain.Id = id;
            await _bookingsRepository.UpdateAsync(bookingDomain);

            _logger.LogInformation("Booking with ID {BookingId} updated successfully", id);
            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update booking with ID {BookingId}", id);
            throw new ExternalServiceException("Database", "Failed to update booking", ex);
        }
    }

    public async Task<BookingDTO?> CheckInAsync(int id)
    {
        _logger.LogInformation("Checking in booking {BookingId}", id);

        try
        {
            var bookingExists = await _bookingsRepository.ExistsAsync(id);
            if (!bookingExists)
            {
                _logger.LogWarning("Booking {BookingId} not found for check-in", id);
                throw new EntityNotFoundException("Booking", id);
            }

            var booking = await _bookingsRepository.GetByIdAsync(id);
            if (booking is null)
            {
                _logger.LogWarning("Booking {BookingId} not found in retrieval step for check-in", id);
                throw new EntityNotFoundException("Booking", id);
            }

            if (DateTime.UtcNow.Date < booking.DateFrom.Date)
            {
                _logger.LogWarning("Attempted early check-in for booking {BookingId}", id);
                throw new BusinessRuleViolationException(
                    "EARLY_CHECKIN_NOT_ALLOWED",
                    $"Cannot check in before booking start date: {booking.DateFrom:yyyy-MM-dd}");
            }

            if (booking.IsCheckedIn)
            {
                _logger.LogWarning("Attempted to check-in booking {BookingId} that is already checked in", id);
                throw new BusinessRuleViolationException(
                    "ALREADY_CHECKED_IN",
                    "Booking has already been checked in");
            }

            if (DateTime.UtcNow.Date > booking.DateTo.Date)
            {
                _logger.LogWarning("Attempted late check-in for expired booking {BookingId}", id);
                throw new BusinessRuleViolationException(
                    "BOOKING_EXPIRED",
                    $"Cannot check in after booking end date: {booking.DateTo:yyyy-MM-dd}");
            }

            await _bookingsRepository.CheckInAsync(id);
            _logger.LogInformation("Successfully checked in booking {BookingId}", id);
            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in booking {BookingId}", id);
            throw new ExternalServiceException("Database", "Failed to check in booking", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting booking {BookingId}", id);

        try
        {
            var bookingExists = await _bookingsRepository.ExistsAsync(id);
            if (!bookingExists)
            {
                _logger.LogWarning("Booking {BookingId} not found when attempting to delete", id);
                throw new EntityNotFoundException("Booking", id);
            }

            var booking = await _bookingsRepository.GetByIdAsync(id);
            if (booking is null)
            {
                _logger.LogWarning("Booking {BookingId} not found in retrieval step for deletion", id);
                throw new EntityNotFoundException("Booking", id);
            }

            if (DateTime.UtcNow.Date >= booking.DateFrom.Date)
            {
                _logger.LogWarning("Cannot delete booking {BookingId}: already started", id);
                throw new BusinessRuleViolationException(
                    "CANNOT_DELETE_ACTIVE_BOOKING",
                    "Cannot delete a booking that has already started or is in progress");
            }

            if (booking.IsCheckedIn)
            {
                _logger.LogWarning("Cannot delete booking {BookingId}: already checked in", id);
                throw new BusinessRuleViolationException(
                    "CANNOT_DELETE_CHECKED_IN_BOOKING",
                    "Cannot delete a booking that has been checked in");
            }

            await _bookingsRepository.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted booking {BookingId}", id);
            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking {BookingId}", id);
            throw new ExternalServiceException("Database", "Failed to delete booking", ex);
        }
    }

    public async Task<bool> CheckAvailabilityAsync(int apartmentId, DateTime dateFrom, DateTime dateTo)
    {
        _logger.LogInformation("Checking availability for apartment {ApartmentId} from {DateFrom} to {DateTo}", apartmentId, dateFrom, dateTo);

        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                _logger.LogWarning("Apartment {ApartmentId} not found when checking availability", apartmentId);
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            var adjustedDateFrom = ConfigureDateTimeForBooking(dateFrom, apartment.Establishment.CheckInTime);
            var adjustedDateTo = ConfigureDateTimeForBooking(dateTo, apartment.Establishment.CheckOutTime);

            ValidateBookingFilterDates(adjustedDateFrom, adjustedDateTo);

            var isAvailable = await _bookingsRepository.IsApartmentAvailableAsync(apartmentId, adjustedDateFrom, adjustedDateTo);
            _logger.LogInformation("Availability check for apartment {ApartmentId}: {IsAvailable}", apartmentId, isAvailable);
            return isAvailable;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for apartment {ApartmentId}", apartmentId);
            throw new ExternalServiceException("Database", "Failed to check apartment availability", ex);
        }
    }

    public async Task<List<(DateTime DateFrom, DateTime DateTo)>> GetBookedDatesAsync(int apartmentId)
    {
        _logger.LogInformation("Getting booked dates for apartment {ApartmentId}", apartmentId);

        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                _logger.LogWarning("Apartment {ApartmentId} not found when getting booked dates", apartmentId);
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            var dates = await _bookingsRepository.GetBookedDatesAsync(apartmentId);
            _logger.LogInformation("Retrieved {Count} booked date ranges for apartment {ApartmentId}", dates.Count, apartmentId);
            return dates;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booked dates for apartment {ApartmentId}", apartmentId);
            throw new ExternalServiceException("Database", "Failed to retrieve booked dates", ex);
        }
    }

    public async Task<ApartmentAvailabilityDTO> GetApartmentAvailabilityAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogInformation("Getting availability for apartment {ApartmentId} from {StartDate} to {EndDate}", apartmentId, startDate, endDate);

        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                _logger.LogWarning("Apartment {ApartmentId} not found when checking availability", apartmentId);
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
                ValidateBookingFilterDates(adjustedStartDate.Value, adjustedEndDate.Value);
            }

            var bookedDays = await _bookingsRepository.GetBookedDaysAsync(apartmentId, adjustedStartDate, adjustedEndDate);
            var bookedRanges = await _bookingsRepository.GetBookedDateRangesAsync(apartmentId, adjustedStartDate, adjustedEndDate);

            var result = new ApartmentAvailabilityDTO
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

            _logger.LogInformation("Successfully retrieved availability for apartment {ApartmentId}", apartmentId);
            return result;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving availability for apartment {ApartmentId}", apartmentId);
            throw new ExternalServiceException("Database", "Failed to retrieve apartment availability", ex);
        }
    }

    private DateTime ConfigureDateTimeForBooking(DateTime providedDateTime, TimeOnly timeToSet)
    {
        _logger.LogInformation("Configuring booking datetime for {Date} with time {Time}", providedDateTime, timeToSet);
        try
        {
            var dateOnly = DateOnly.FromDateTime(providedDateTime);
            return new DateTime(dateOnly, timeToSet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure booking datetime");
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

    private void ValidateBookingFilterDates(DateTime dateFrom, DateTime dateTo)
    {
        if (dateFrom >= dateTo)
        {
            throw new BusinessRuleViolationException(
                "INVALID_DATE_RANGE",
                "DateTo date must be after DateFrom date");
        }

        if (dateFrom.Date < DateTime.UtcNow.Date)
        {
            throw new BusinessRuleViolationException(
                "PAST_DATE_BOOKING",
                "Cannot check apartment availability for past dates");
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