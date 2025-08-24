using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Helpers;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookIt.BLL.Services;

public class ApartmentsService : IApartmentsService
{
    private const string BlobContainerName = "apartments";

    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly RedisSettings _redisSettings;
    private readonly IImagesService _imagesService;
    private readonly IRatingsService _ratingsService;
    private readonly ILogger<ApartmentsService> _logger;
    private readonly ImagesRepository _imagesRepository;
    private readonly BookingsRepository _bookingsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;

    public ApartmentsService(
        IMapper mapper,
        ICacheService cacheService,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ILogger<ApartmentsService> logger,
        ImagesRepository imagesRepository,
        IOptions<RedisSettings> redisoptions,
        BookingsRepository bookingsRepository,
        ApartmentsRepository apartmentsRepository)
    {
        _redisSettings = redisoptions.Value;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _imagesService = imagesService ?? throw new ArgumentNullException(nameof(imagesService));
        _ratingsService = ratingsService ?? throw new ArgumentNullException(nameof(ratingsService));
        _imagesRepository = imagesRepository ?? throw new ArgumentNullException(nameof(imagesRepository));
        _bookingsRepository = bookingsRepository ?? throw new ArgumentNullException(nameof(bookingsRepository));
        _apartmentsRepository = apartmentsRepository ?? throw new ArgumentNullException(nameof(apartmentsRepository));
    }

    public async Task<IEnumerable<ApartmentDTO>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all apartments");
            var apartmentsDomain = await _apartmentsRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} apartments", apartmentsDomain.Count());
            return _mapper.Map<IEnumerable<ApartmentDTO>>(apartmentsDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve apartments");
            throw new ExternalServiceException("Database", "Failed to retrieve apartments", ex);
        }
    }

    public async Task<ApartmentDTO?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving apartment with ID {ApartmentId}", id);

        var cacheKey = CacheKeys.ApartmentById(id);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    var apartmentDomain = await _apartmentsRepository.GetByIdAsync(id);
                    if (apartmentDomain is null)
                    {
                        _logger.LogWarning("Apartment with ID {ApartmentId} not found", id);
                        throw new EntityNotFoundException("Apartment", id);
                    }
                    _logger.LogInformation("Retrieved apartment with ID {ApartmentId}", id);
                    return _mapper.Map<ApartmentDTO>(apartmentDomain);
                }
                catch (BookItBaseException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve apartment with ID {ApartmentId}", id);
                    throw new ExternalServiceException("Database", "Failed to retrieve apartment", ex);
                }
            },
            TimeSpan.FromMinutes(_redisSettings.Expiration.Apartments)
        );
    }

    public async Task<ApartmentDTO?> CreateAsync(ApartmentDTO dto, int requestorId)
    {
        try
        {
            _logger.LogInformation("Creating new apartment");

            if (!await _apartmentsRepository.IsUserEligibleToCreateAsync(dto.EstablishmentId, requestorId))
            {
                _logger.LogWarning("User {UserId} is not eligible to create apartment in establishment {EstablishmentId}.", requestorId, dto.EstablishmentId);
                throw new UnauthorizedOperationException("You are not authorized to create this apartment");
            }

            var apartmentDomain = _mapper.Map<Apartment>(dto);
            var addedApartment = await _apartmentsRepository.AddAsync(apartmentDomain);

            await InvalidateApartmentCachesAsync(addedApartment.Id, addedApartment.EstablishmentId);

            dto.Photos ??= [];

            _logger.LogInformation("Processing {Count} images for new apartment ID {ApartmentId}", dto.Photos.Count(), addedApartment.Id);
            await ProcessApartmentImagesAsync(addedApartment.Id, dto.Photos);

            _logger.LogInformation("Successfully created apartment with ID {ApartmentId}", addedApartment.Id);
            return await GetByIdAsync(addedApartment.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create apartment");
            throw new ExternalServiceException("Database", "Failed to create apartment", ex);
        }
    }

    public async Task<ApartmentDTO?> UpdateAsync(int id, ApartmentDTO dto, int requestorId)
    {
        try
        {
            _logger.LogInformation("Updating apartment with ID {ApartmentId}", id);
            var apartmentExists = await _apartmentsRepository.ExistsAsync(id);
            if (!apartmentExists)
            {
                _logger.LogWarning("Apartment with ID {ApartmentId} not found for update", id);
                throw new EntityNotFoundException("Apartment", id);
            }

            if (!await _apartmentsRepository.IsUserEligibleToUpdateAsync(id, dto.EstablishmentId, requestorId))
            {
                _logger.LogWarning("User {UserId} is not authorized to update apartment with ID {ApartmentId}", requestorId, id);
                throw new UnauthorizedOperationException("You are not authorized to update this apartment");
            }

            var apartmentDomain = _mapper.Map<Apartment>(dto);
            apartmentDomain.Id = id;
            await InvalidateApartmentCachesAsync(id, dto.EstablishmentId);
            await _apartmentsRepository.UpdateAsync(apartmentDomain);

            await ProcessApartmentImagesAsync(id, dto.Photos);

            _logger.LogInformation("Successfully updated apartment with ID {ApartmentId}", id);
            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update apartment with ID {ApartmentId}", id);
            throw new ExternalServiceException("Database", "Failed to update apartment", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id, int requestorId)
    {
        try
        {
            _logger.LogInformation("Deleting apartment with ID {ApartmentId}", id);
            var apartment = await _apartmentsRepository.GetByIdAsync(id);
            if (apartment is null)
            {
                _logger.LogWarning("Apartment with ID {ApartmentId} not found for deletion", id);
                throw new EntityNotFoundException("Apartment", id);
            }

            if (!await _apartmentsRepository.IsUserEligibleToDeleteAsync(id, requestorId))
            {
                _logger.LogWarning("User {UserId} is not authorized to delete apartment with ID {ApartmentId}", requestorId, id);
                throw new UnauthorizedOperationException("You are not authorized to delete this apartment");
            }

            await ValidateApartmentCanBeDeletedAsync(id);

            await InvalidateApartmentCachesAsync(id, apartment.EstablishmentId);

            await RemoveAllApartmentImagesAsync(id);

            await _apartmentsRepository.DeleteAsync(id);

            _logger.LogInformation("Successfully deleted apartment with ID {ApartmentId}", id);
            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete apartment with ID {ApartmentId}", id);
            throw new ExternalServiceException("Database", "Failed to delete apartment", ex);
        }
    }

    public async Task<PagedResultDTO<ApartmentDTO>> GetPagedByEstablishmentIdAsync(int establishmentId, int page, int pageSize)
    {
        _logger.LogInformation("Retrieving apartments for establishment {EstablishmentId}, page {Page}, size {PageSize}", establishmentId, page, pageSize);

        var cacheKey = CacheKeys.ApartmentsByEstablishmentId(establishmentId, page, pageSize);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    ValidatePaginationParameters(page, pageSize);

                    var (apartments, totalCount) = await _apartmentsRepository.GetPagedByEstablishmentIdAsync(establishmentId, page, pageSize);
                    var apartmentsDto = _mapper.Map<IEnumerable<ApartmentDTO>>(apartments);

                    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    _logger.LogInformation("Retrieved {Count} apartments for establishment {EstablishmentId}", apartmentsDto.Count(), establishmentId);

                    return new PagedResultDTO<ApartmentDTO>
                    {
                        Items = apartmentsDto.ToList(),
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    };
                }
                catch (BookItBaseException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve paged apartments for establishment {EstablishmentId}", establishmentId);
                    throw new ExternalServiceException("Database", "Failed to retrieve paged apartments", ex);
                }
            },
            TimeSpan.FromMinutes(_redisSettings.Expiration.Apartments)
        );
    }

    private async Task InvalidateApartmentCachesAsync(int apartmentId, int establishmentId)
    {
        var invalidationTasks = new List<Task>
        {
            _cacheService.RemoveAsync(CacheKeys.ApartmentById(apartmentId)),
            _cacheService.RemoveByPatternAsync(CacheKeys.ApartmentsByEstablishmentId(establishmentId)),
        };

        await Task.WhenAll(invalidationTasks);
        _logger.LogInformation("Invalidated apartments caches for ApartmentId={ApartmentId}, EstablishmentId={EstablishmentId}",
            apartmentId, establishmentId);
    }

    private void ValidatePaginationParameters(int page, int pageSize)
    {
        if (page <= 0)
            throw new BusinessRuleViolationException("INVALID_PAGE", "Page number must be greater than 0");
        if (pageSize <= 0)
            throw new BusinessRuleViolationException("INVALID_PAGE_SIZE", "Page size must be greater than 0");
        if (pageSize > 100)
            throw new BusinessRuleViolationException("PAGE_SIZE_TOO_LARGE", "Page size cannot exceed 100 items");
    }

    private async Task ValidateApartmentCanBeDeletedAsync(int apartmentId)
    {
        try
        {
            _logger.LogInformation("Validating if apartment ID {ApartmentId} can be deleted", apartmentId);
            await ValidateNoActiveBookingsAsync(apartmentId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate apartment deletion eligibility for apartment ID {ApartmentId}", apartmentId);
            throw new ExternalServiceException("Validation", "Failed to validate apartment deletion eligibility", ex);
        }
    }

    private async Task ValidateNoActiveBookingsAsync(int apartmentId)
    {
        var activeBookings = await _bookingsRepository.GetActiveAndFutureBookingsAsync(apartmentId);

        if (activeBookings.Any())
        {
            _logger.LogWarning("Apartment ID {ApartmentId} has {Count} active/future bookings", apartmentId, activeBookings.Count());
            throw new BusinessRuleViolationException(
                "APARTMENT_HAS_ACTIVE_BOOKINGS",
                $"Cannot delete apartment with {activeBookings.Count()} active or future booking(s). Please wait until all bookings are completed or cancel them first.");
        }
    }

    private async Task ProcessApartmentImagesAsync(int apartmentId, IEnumerable<ImageDTO> photos)
    {
        photos ??= [];

        _logger.LogInformation("Processing apartment images for apartment ID {ApartmentId}", apartmentId);

        Action<Image> setApartmentIdDelegate = image => image.ApartmentId = apartmentId;

        var existingImageIds = (await _imagesRepository.GetApartmentImagesAsync(apartmentId))
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

        if (idsOfPhotosToRemove.Any())
            _logger.LogInformation("Removing {Count} images from apartment ID {ApartmentId}", idsOfPhotosToRemove.Count, apartmentId);

        if (photosToAdd.Any())
            _logger.LogInformation("Adding {Count} new images to apartment ID {ApartmentId}", photosToAdd.Count, apartmentId);

        var tasks = new List<Task>();

        if (idsOfPhotosToRemove.Any())
            tasks.Add(_imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName));

        if (photosToAdd.Any())
            tasks.Add(_imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setApartmentIdDelegate));

        if (tasks.Any())
            await Task.WhenAll(tasks);
    }

    private async Task RemoveAllApartmentImagesAsync(int apartmentId)
    {
        try
        {
            _logger.LogInformation("Removing all images for apartment ID {ApartmentId}", apartmentId);
            var existingImageIds = (await _imagesRepository.GetApartmentImagesAsync(apartmentId))
                .Select(image => image.Id)
                .ToList();

            if (existingImageIds.Any())
            {
                _logger.LogInformation("Deleting {Count} images for apartment ID {ApartmentId}", existingImageIds.Count, apartmentId);
                await _imagesService.DeleteImagesAsync(existingImageIds, BlobContainerName);
            }
            else
            {
                _logger.LogInformation("No images found to delete for apartment ID {ApartmentId}", apartmentId);
            }
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove apartment images for apartment ID {ApartmentId}", apartmentId);
            throw new ExternalServiceException("ImageProcessing", "Failed to remove apartment images", ex);
        }
    }
}
