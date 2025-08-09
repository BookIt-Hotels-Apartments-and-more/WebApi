using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Constants;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using System.Linq.Expressions;

namespace BookIt.BLL.Services;

public class EstablishmentsService : IEstablishmentsService
{
    private const string BlobContainerName = "establishments";

    private readonly IMapper _mapper;
    private readonly IImagesService _imagesService;
    private readonly IRatingsService _ratingsService;
    private readonly ImagesRepository _imagesRepository;
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
        IGeolocationService geolocationService,
        ApartmentsRepository apartmentsRepository,
        IClassificationService classificationService,
        EstablishmentsRepository establishmentsRepository)
    {
        _mapper = mapper;
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
        try
        {
            var establishmentsDomain = await _establishmentsRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<EstablishmentDTO>>(establishmentsDomain);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve establishments", ex);
        }
    }

    public async Task<EstablishmentDTO?> GetByIdAsync(int id)
    {
        try
        {
            var establishmentDomain = await _establishmentsRepository.GetByIdAsync(id);
            if (establishmentDomain is null)
            {
                throw new EntityNotFoundException("Establishment", id);
            }

            return _mapper.Map<EstablishmentDTO>(establishmentDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve establishment", ex);
        }
    }

    public async Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto)
    {
        try
        {
            Task<GeolocationDTO?> addGeolocationTask = _geolocationService.CreateAsync(dto.Geolocation);
            Task<VibeType?> classifyVibeTask = _classificationService.ClassifyEstablishmentVibeAsync(dto);

            await Task.WhenAll(addGeolocationTask, classifyVibeTask);

            var geolocationResult = addGeolocationTask.Result;
            if (geolocationResult?.Id is null)
                throw new BusinessRuleViolationException("GEOLOCATION_CREATION_FAILED", "Failed to create geolocation for establishment");

            var establishmentDomain = _mapper.Map<Establishment>(dto);
            establishmentDomain.GeolocationId = geolocationResult.Id;
            establishmentDomain.Vibe = classifyVibeTask.Result ?? VibeType.None;

            var addedEstablishment = await _establishmentsRepository.AddAsync(establishmentDomain);

            if (dto.Photos?.Any() == true)
                await ProcessEstablishmentImagesAsync(addedEstablishment.Id, dto.Photos);

            return await GetByIdAsync(addedEstablishment.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to create establishment", ex);
        }
    }

    public async Task<EstablishmentDTO?> UpdateAsync(int id, EstablishmentDTO dto)
    {
        try
        {
            var establishmentExists = await _establishmentsRepository.ExistsAsync(id);
            if (!establishmentExists)
                throw new EntityNotFoundException("Establishment", id);

            var establishmentDomain = _mapper.Map<Establishment>(dto);
            establishmentDomain.Id = id;

            GeolocationDTO? updateGeolocationTask = await _geolocationService.UpdateEstablishmentGeolocationAsync(id, dto.Geolocation);
            VibeType? updateVibeTask = await _classificationService.UpdateEstablishmentVibeAsync(id, dto);

            if (updateGeolocationTask?.Id is not null)
                establishmentDomain.GeolocationId = updateGeolocationTask?.Id;

            if (updateVibeTask is not null)
                establishmentDomain.Vibe = updateVibeTask;

            await _establishmentsRepository.UpdateAsync(establishmentDomain);

            await ProcessEstablishmentImagesAsync(id, dto.Photos);

            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update establishment", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var establishmentExists = await _establishmentsRepository.ExistsAsync(id);
            if (!establishmentExists)
                throw new EntityNotFoundException("Establishment", id);

            await ValidateEstablishmentCanBeDeletedAsync(id);

            await RemoveAllEstablishmentImagesAsync(id);

            await _geolocationService.DeleteEstablishmentGeolocationAsync(id);

            await _establishmentsRepository.DeleteAsync(id);

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to delete establishment", ex);
        }
    }

    public async Task<PagedResultDTO<EstablishmentDTO>> GetFilteredAsync(EstablishmentFilterDTO filter)
    {
        try
        {
            ValidateFilterParameters(filter);

            Expression<Func<Establishment, bool>> predicate = BuildDatabasePredicate(filter);

            var (establishments, totalCount) = await _establishmentsRepository.GetFilteredAsync(
                predicate,
                filter.Page,
                filter.PageSize);

            var establishmentsDto = _mapper.Map<IEnumerable<EstablishmentDTO>>(establishments);

            var filteredEstablishments = ApplyInMemoryFilters(establishmentsDto, filter);

            var finalCount = filteredEstablishments.Count();
            var totalPages = (int)Math.Ceiling(finalCount / (double)filter.PageSize);

            return new PagedResultDTO<EstablishmentDTO>
            {
                Items = filteredEstablishments.ToList(),
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
            throw new ExternalServiceException("Database", "Failed to get filtered establishments", ex);
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

        if (!string.IsNullOrWhiteSpace(filter.Name)) predicate = predicate.And(e => e.Name.Contains(filter.Name));
        if (filter.Type.HasValue) predicate = predicate.And(e => e.Type == filter.Type.Value);
        if (filter.Features.HasValue) predicate = predicate.And(e => (e.Features & filter.Features.Value) == filter.Features.Value);
        if (filter.OwnerId.HasValue) predicate = predicate.And(e => e.OwnerId == filter.OwnerId.Value);

        return predicate;
    }

    private IEnumerable<EstablishmentDTO> ApplyInMemoryFilters(IEnumerable<EstablishmentDTO> establishments, EstablishmentFilterDTO filter)
    {
        var filteredEstablishments = establishments;

        if (!string.IsNullOrWhiteSpace(filter.Country)) filteredEstablishments = filteredEstablishments
                .Where(e => e.Geolocation?.Country != null && e.Geolocation.Country.Contains(filter.Country, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.City)) filteredEstablishments = filteredEstablishments
                .Where(e => e.Geolocation?.City != null && e.Geolocation.City.Contains(filter.City, StringComparison.OrdinalIgnoreCase));

        if (filter.MinRating.HasValue) filteredEstablishments = filteredEstablishments.Where(e => e.Rating?.GeneralRating >= filter.MinRating.Value);
        if (filter.MaxRating.HasValue) filteredEstablishments = filteredEstablishments.Where(e => e.Rating?.GeneralRating <= filter.MaxRating.Value);
        if (filter.MinPrice.HasValue) filteredEstablishments = filteredEstablishments.Where(e => e.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue) filteredEstablishments = filteredEstablishments.Where(e => e.Price <= filter.MaxPrice.Value);

        return filteredEstablishments;
    }

    private async Task ProcessEstablishmentImagesAsync(int establishmentId, IEnumerable<ImageDTO>? photos)
    {
        if (photos?.Any() != true) return;

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
                    $"Apartment '{apartment.Name}' - Booking #{b.Id}: {b.DateFrom:yyyy-MM-dd} to {b.DateTo:yyyy-MM-dd} (User: {b.User.Username})");

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