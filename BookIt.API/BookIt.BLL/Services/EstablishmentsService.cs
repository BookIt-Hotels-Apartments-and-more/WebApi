using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Constants;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BookIt.BLL.Services;

public class EstablishmentsService : IEstablishmentsService
{
    private const string BlobContainerName = "establishments";

    private readonly IMapper _mapper;
    private readonly IImagesService _imagesService;
    private readonly IRatingsService _ratingsService;
    private readonly ImagesRepository _imagesRepository;
    private readonly ILogger<EstablishmentsService> _logger;
    private readonly BookingsRepository _bookingsRepository;
    private readonly IGeolocationService _geolocationService;
    private readonly ApartmentsRepository _apartmentsRepository;
    private readonly IClassificationService _classificationService;
    private readonly EstablishmentsRepository _establishmentsRepository;

    public EstablishmentsService(
        IMapper mapper,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ImagesRepository imagesRepository,
        BookingsRepository bookingsRepository,
        ILogger<EstablishmentsService> logger,
        IGeolocationService geolocationService,
        ApartmentsRepository apartmentsRepository,
        IClassificationService classificationService,
        EstablishmentsRepository establishmentsRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _imagesService = imagesService;
        _ratingsService = ratingsService;
        _imagesRepository = imagesRepository;
        _bookingsRepository = bookingsRepository;
        _geolocationService = geolocationService;
        _apartmentsRepository = apartmentsRepository;
        _classificationService = classificationService;
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<IEnumerable<EstablishmentDTO>> GetAllAsync()
    {
        _logger.LogInformation("Start GetAllAsync");
        try
        {
            var establishmentsDomain = await _establishmentsRepository.GetAllAsync();
            var result = _mapper.Map<IEnumerable<EstablishmentDTO>>(establishmentsDomain);
            _logger.LogInformation("Successfully retrieved {Count} establishments", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve establishments");
            throw new ExternalServiceException("Database", "Failed to retrieve establishments", ex);
        }
    }

    public async Task<EstablishmentDTO?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Start GetByIdAsync for Establishment Id: {Id}", id);
        try
        {
            var establishmentDomain = await _establishmentsRepository.GetByIdAsync(id);
            if (establishmentDomain is null)
            {
                _logger.LogWarning("Establishment with Id {Id} not found", id);
                throw new EntityNotFoundException("Establishment", id);
            }

            var result = _mapper.Map<EstablishmentDTO>(establishmentDomain);
            _logger.LogInformation("Successfully retrieved establishment with Id {Id}", id);
            return result;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve establishment with Id {Id}", id);
            throw new ExternalServiceException("Database", "Failed to retrieve establishment", ex);
        }
    }

    public async Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto)
    {
        _logger.LogInformation("Start CreateAsync for establishment with name: {Name}", dto.Name);
        try
        {
            Task<GeolocationDTO?> addGeolocationTask = _geolocationService.CreateAsync(dto.Geolocation);
            Task<VibeType?> classifyVibeTask = _classificationService.ClassifyEstablishmentVibeAsync(dto);

            await Task.WhenAll(addGeolocationTask, classifyVibeTask);

            var geolocationResult = addGeolocationTask.Result;
            if (geolocationResult?.Id is null)
            {
                _logger.LogError("Failed to create geolocation for establishment");
                throw new BusinessRuleViolationException("GEOLOCATION_CREATION_FAILED", "Failed to create geolocation for establishment");
            }

            var establishmentDomain = _mapper.Map<Establishment>(dto);
            establishmentDomain.GeolocationId = geolocationResult.Id;
            establishmentDomain.Vibe = classifyVibeTask.Result ?? VibeType.None;

            var addedEstablishment = await _establishmentsRepository.AddAsync(establishmentDomain);

            dto.Photos ??= [];
            await ProcessEstablishmentImagesAsync(addedEstablishment.Id, dto.Photos);

            _logger.LogInformation("Successfully created establishment with Id {Id}", addedEstablishment.Id);

            return await GetByIdAsync(addedEstablishment.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create establishment");
            throw new ExternalServiceException("Database", "Failed to create establishment", ex);
        }
    }

    public async Task<EstablishmentDTO?> UpdateAsync(int id, EstablishmentDTO dto)
    {
        _logger.LogInformation("Start UpdateAsync for establishment Id: {Id}", id);
        try
        {
            var establishmentExists = await _establishmentsRepository.ExistsAsync(id);
            if (!establishmentExists)
            {
                _logger.LogWarning("Establishment with Id {Id} not found for update", id);
                throw new EntityNotFoundException("Establishment", id);
            }

            var establishmentDomain = _mapper.Map<Establishment>(dto);
            establishmentDomain.Id = id;

            GeolocationDTO? updateGeolocationTask = await _geolocationService.UpdateEstablishmentGeolocationAsync(id, dto.Geolocation);
            VibeType? updateVibeTask = await _classificationService.UpdateEstablishmentVibeAsync(id, dto);

            if (updateGeolocationTask?.Id is not null)
                establishmentDomain.GeolocationId = updateGeolocationTask?.Id;

            if (updateVibeTask is not null)
                establishmentDomain.Vibe = updateVibeTask;

            var currentEstablishmentRatingId = await _establishmentsRepository.GetEstablishmentRatingAsync(id);
            establishmentDomain.ApartmentRatingId = currentEstablishmentRatingId;

            await _establishmentsRepository.UpdateAsync(establishmentDomain);

            await ProcessEstablishmentImagesAsync(id, dto.Photos);

            _logger.LogInformation("Successfully updated establishment with Id {Id}", id);

            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update establishment with Id {Id}", id);
            throw new ExternalServiceException("Database", "Failed to update establishment", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Start DeleteAsync for establishment Id: {Id}", id);
        try
        {
            var establishmentExists = await _establishmentsRepository.ExistsAsync(id);
            if (!establishmentExists)
            {
                _logger.LogWarning("Establishment with Id {Id} not found for deletion", id);
                throw new EntityNotFoundException("Establishment", id);
            }

            await ValidateEstablishmentCanBeDeletedAsync(id);

            await RemoveAllEstablishmentImagesAsync(id);

            await _geolocationService.DeleteEstablishmentGeolocationAsync(id);

            await _establishmentsRepository.DeleteAsync(id);

            _logger.LogInformation("Successfully deleted establishment with Id {Id}", id);

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete establishment with Id {Id}", id);
            throw new ExternalServiceException("Database", "Failed to delete establishment", ex);
        }
    }

    public async Task<PagedResultDTO<EstablishmentDTO>> GetFilteredAsync(EstablishmentFilterDTO filter)
    {
        _logger.LogInformation("Start filtering establishments with filter {@Filter}", filter);
        try
        {
            ValidateFilterParameters(filter);

            Expression<Func<Establishment, bool>> predicate = BuildDatabasePredicate(filter);

            var (establishments, totalCount) = await _establishmentsRepository.GetFilteredAsync(
                predicate,
                filter.Page,
                filter.PageSize);

            var establishmentsDto = _mapper.Map<IEnumerable<EstablishmentDTO>>(establishments);

            var finalCount = establishmentsDto.Count();
            var totalPages = (int)Math.Ceiling(finalCount / (double)filter.PageSize);

            _logger.LogInformation("Filtered result count: {Count}, total pages: {TotalPages}", finalCount, totalPages);

            return new PagedResultDTO<EstablishmentDTO>
            {
                Items = establishmentsDto,
                PageNumber = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = finalCount,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get filtered establishments");
            throw new ExternalServiceException("Database", "Failed to get filtered establishments", ex);
        }
    }

    public async Task<IEnumerable<TrendingEstablishmentDTO>> GetTrendingAsync(int count = 10, int? periodInDays = null)
    {
        try
        {
            _logger.LogInformation("Start getting top {Count} trending establishments {Period} ",
                count, periodInDays is null ? "ever" : $"for the past {periodInDays} days");

            if (count <= 0)
                throw new BusinessRuleViolationException("INVALID_COUNT", "Count must be greater than 0");

            if (periodInDays.HasValue && periodInDays <= 0)
                throw new BusinessRuleViolationException("INVALID_PERIOD", "Period in days must be greater than 0");

            var establishmentsAndBookingsCount = await _establishmentsRepository.GetTrendingAsync(count, periodInDays);
            var trendingEstablishmentsDto = establishmentsAndBookingsCount
                .Select(x =>
                {
                    var dto = _mapper.Map<TrendingEstablishmentDTO>(x.Establishment);
                    dto.BookingsCount = x.BookingCount;
                    return dto;
                })
                .ToList();

            _logger.LogInformation("Successfully retrieved top {Count} trending establishments", trendingEstablishmentsDto.Count());
            return trendingEstablishmentsDto;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve trending establishments");
            throw new ExternalServiceException("Database", "Failed to retrieve trending establishments", ex);
        }
    }

    private void ValidateFilterParameters(EstablishmentFilterDTO filter)
    {
        if (filter.Page <= 0)
            throw new BusinessRuleViolationException("INVALID_PAGE", "Page number must be greater than 0");

        if (filter.PageSize <= 0)
            throw new BusinessRuleViolationException("INVALID_PAGE_SIZE", "Page size must be greater than 0");

        if (filter.PageSize > 100)
            throw new BusinessRuleViolationException("PAGE_SIZE_TOO_LARGE", "Page size cannot exceed 100 items");

        if (filter.MinRating.HasValue && (filter.MinRating < RatingConstants.MinRating || filter.MinRating > RatingConstants.MaxRating))
            throw new BusinessRuleViolationException("INVALID_MIN_RATING", "Minimum rating must be between 0 and 5");

        if (filter.MaxRating.HasValue && (filter.MaxRating < RatingConstants.MinRating || filter.MaxRating > RatingConstants.MaxRating))
            throw new BusinessRuleViolationException("INVALID_MAX_RATING", "Maximum rating must be between 0 and 5");

        if (filter.MinRating.HasValue && filter.MaxRating.HasValue && filter.MinRating > filter.MaxRating)
            throw new BusinessRuleViolationException("INVALID_RATING_RANGE", "Minimum rating cannot be greater than maximum rating");

        if (filter.MinPrice.HasValue && filter.MinPrice < 0)
            throw new BusinessRuleViolationException("INVALID_MIN_PRICE", "Minimum price cannot be negative");

        if (filter.MaxPrice.HasValue && filter.MaxPrice < 0)
            throw new BusinessRuleViolationException("INVALID_MAX_PRICE", "Maximum price cannot be negative");

        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice)
            throw new BusinessRuleViolationException("INVALID_PRICE_RANGE", "Minimum price cannot be greater than maximum price");
    }

    private Expression<Func<Establishment, bool>> BuildDatabasePredicate(EstablishmentFilterDTO filter)
    {
        Expression<Func<Establishment, bool>> predicate = e => true;

        if (filter.OwnerId.HasValue) predicate = predicate.And(e => e.OwnerId == filter.OwnerId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Country)) predicate = predicate.And(e => e.Geolocation != null &&
                                                                                       e.Geolocation.Country != null &&
                                                                                       e.Geolocation.Country.ToLower().Contains(filter.Country));
        if (!string.IsNullOrWhiteSpace(filter.City))    predicate = predicate.And(e => e.Geolocation != null &&
                                                                                       e.Geolocation.City != null &&
                                                                                       e.Geolocation.City.ToLower().Contains(filter.City));

        if (!string.IsNullOrWhiteSpace(filter.Name)) predicate = predicate.And(e => e.Name.ToLower().Contains(filter.Name));

        if (filter.Vibe.HasValue) predicate = predicate.And(e => e.Vibe == filter.Vibe.Value);

        if (filter.Type.HasValue) predicate = predicate.And(e => e.Type == filter.Type.Value);

        if (filter.Features.HasValue) predicate = predicate.And(e => (e.Features & filter.Features.Value) == filter.Features.Value);

        if (filter.MinRating.HasValue) predicate = predicate.And(e => e.ApartmentRating != null &&
                                                                      e.ApartmentRating.GeneralRating >= filter.MinRating.Value);
        if (filter.MaxRating.HasValue) predicate = predicate.And(e => e.ApartmentRating != null &&
                                                                      e.ApartmentRating.GeneralRating <= filter.MaxRating.Value);

        if (filter.MinPrice.HasValue) predicate = predicate.And(e => e.Apartments.Any() &&
                                                                     e.Apartments.Max(a => a.Price) >= (double)filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue) predicate = predicate.And(e => e.Apartments.Any() &&
                                                                     e.Apartments.Min(a => a.Price) <= (double)filter.MaxPrice.Value);

        return predicate;
    }

    private async Task ProcessEstablishmentImagesAsync(int establishmentId, IEnumerable<ImageDTO>? photos)
    {
        photos ??= [];

        Action<Image> setEstablishmentIdDelegate = image => image.EstablishmentId = establishmentId;

        var existingImageIds = (await _imagesRepository.GetEstablishmentImagesAsync(establishmentId))
            .Select(photo => photo.Id)
            .ToList();

        var idsOfPhotosToKeep = photos
            .Where(photo => photo.Id.HasValue && string.IsNullOrEmpty(photo.Base64Image))
            .Select(photo => photo.Id!.Value)
            .ToList();

        var idsOfPhotosToRemove = existingImageIds
            .Where(id => !idsOfPhotosToKeep.Contains(id))
            .ToList();

        var photosToAdd = photos
            .Where(photo => !photo.Id.HasValue && !string.IsNullOrEmpty(photo.Base64Image))
            .ToList();

        var tasks = new List<Task>();

        if (idsOfPhotosToRemove.Any())
            tasks.Add(_imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName));

        if (photosToAdd.Any())
            tasks.Add(_imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setEstablishmentIdDelegate));

        if (tasks.Any())
            await Task.WhenAll(tasks);
    }

    private async Task RemoveAllEstablishmentImagesAsync(int establishmentId)
    {
        var existingImageIds = (await _imagesRepository.GetEstablishmentImagesAsync(establishmentId))
            .Select(image => image.Id)
            .ToList();

        if (existingImageIds.Any())
            await _imagesService.DeleteImagesAsync(existingImageIds, BlobContainerName);
    }

    private async Task ValidateEstablishmentCanBeDeletedAsync(int establishmentId)
    {
        try
        {
            await ValidateNoActiveBookingsInApartmentsAsync(establishmentId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Validation", "Failed to validate establishment deletion eligibility", ex);
        }
    }

    private async Task ValidateNoActiveBookingsInApartmentsAsync(int establishmentId)
    {
        var apartments = await _apartmentsRepository.GetByEstablishmentIdAsync(establishmentId);

        if (!apartments.Any()) return;

        var apartmentIds = apartments.Select(a => a.Id).ToList();
        var activeBookingsCount = 0;
        var activeBookingDetails = new List<string>();

        foreach (var apartmentId in apartmentIds)
        {
            var activeBookings = await _bookingsRepository.GetActiveAndFutureBookingsAsync(apartmentId);

            if (activeBookings.Any())
            {
                activeBookingsCount += activeBookings.Count();

                var apartment = apartments.First(a => a.Id == apartmentId);

                var bookingDetails = activeBookings.Select(b =>
                    $"Apartment '{apartment.Name}' - Booking #{b.Id}: {b.DateFrom:yyyy-MM-dd} to {b.DateTo:yyyy-MM-dd}");

                activeBookingDetails.AddRange(bookingDetails);
            }
        }

        if (activeBookingsCount > 0)
            throw new BusinessRuleViolationException(
                "ESTABLISHMENT_HAS_ACTIVE_BOOKINGS",
                $"Cannot delete establishment with {activeBookingsCount} active or future booking(s) across {apartments.Count()} apartment(s). " +
                "Please wait until all bookings are completed or cancel them first.",
                new Dictionary<string, object>
                {
                { "ActiveBookingsCount", activeBookingsCount },
                { "ApartmentsCount", apartments.Count() },
                { "ActiveBookings", activeBookingDetails }
                });
    }
}