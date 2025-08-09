using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class ApartmentsService : IApartmentsService
{
    private const string BlobContainerName = "apartments";

    private readonly IMapper _mapper;
    private readonly IImagesService _imagesService;
    private readonly IRatingsService _ratingsService;
    private readonly ImagesRepository _imagesRepository;
    private readonly BookingsRepository _bookingsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;

    public ApartmentsService(
        IMapper mapper,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ImagesRepository imagesRepository,
        BookingsRepository bookingsRepository,
        ApartmentsRepository apartmentsRepository)
    {
        _mapper = mapper;
        _imagesService = imagesService;
        _ratingsService = ratingsService;
        _imagesRepository = imagesRepository;
        _bookingsRepository = bookingsRepository;
        _apartmentsRepository = apartmentsRepository;
    }

    public async Task<IEnumerable<ApartmentDTO>> GetAllAsync()
    {
        try
        {
            var apartmentsDomain = await _apartmentsRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ApartmentDTO>>(apartmentsDomain);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve apartments", ex);
        }
    }

    public async Task<ApartmentDTO?> GetByIdAsync(int id)
    {
        try
        {
            var apartmentDomain = await _apartmentsRepository.GetByIdAsync(id);
            if (apartmentDomain is null)
            {
                throw new EntityNotFoundException("Apartment", id);
            }

            return _mapper.Map<ApartmentDTO>(apartmentDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve apartment", ex);
        }
    }

    public async Task<ApartmentDTO?> CreateAsync(ApartmentDTO dto)
    {
        try
        {
            var apartmentDomain = _mapper.Map<Apartment>(dto);
            var addedApartment = await _apartmentsRepository.AddAsync(apartmentDomain);

            if (dto.Photos?.Any() == true)
                await ProcessApartmentImagesAsync(addedApartment.Id, dto.Photos);

            return await GetByIdAsync(addedApartment.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to create apartment", ex);
        }
    }

    public async Task<ApartmentDTO?> UpdateAsync(int id, ApartmentDTO dto)
    {
        try
        {
            var apartmentExists = await _apartmentsRepository.ExistsAsync(id);
            if (!apartmentExists)
                throw new EntityNotFoundException("Apartment", id);

            var apartmentDomain = _mapper.Map<Apartment>(dto);
            apartmentDomain.Id = id;
            await _apartmentsRepository.UpdateAsync(apartmentDomain);

            await ProcessApartmentImagesAsync(id, dto.Photos);

            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update apartment", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var apartmentExists = await _apartmentsRepository.ExistsAsync(id);
            if (!apartmentExists)
                throw new EntityNotFoundException("Apartment", id);

            await ValidateApartmentCanBeDeletedAsync(id);

            await RemoveAllApartmentImagesAsync(id);

            await _apartmentsRepository.DeleteAsync(id);

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to delete apartment", ex);
        }
    }

    public async Task<PagedResultDTO<ApartmentDTO>> GetPagedByEstablishmentIdAsync(int establishmentId, int page, int pageSize)
    {
        try
        {
            ValidatePaginationParameters(page, pageSize);

            var (apartments, totalCount) = await _apartmentsRepository.GetPagedByEstablishmentIdAsync(establishmentId, page, pageSize);
            var apartmentsDto = _mapper.Map<IEnumerable<ApartmentDTO>>(apartments);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

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
            throw new ExternalServiceException("Database", "Failed to retrieve paged apartments", ex);
        }
    }

    private void ValidatePaginationParameters(int page, int pageSize)
    {
        if (page <= 0)
        {
            throw new BusinessRuleViolationException("INVALID_PAGE", "Page number must be greater than 0");
        }

        if (pageSize <= 0)
        {
            throw new BusinessRuleViolationException("INVALID_PAGE_SIZE", "Page size must be greater than 0");
        }

        if (pageSize > 100)
        {
            throw new BusinessRuleViolationException("PAGE_SIZE_TOO_LARGE", "Page size cannot exceed 100 items");
        }
    }

    private async Task ValidateApartmentCanBeDeletedAsync(int apartmentId)
    {
        try
        {
            await ValidateNoActiveBookingsAsync(apartmentId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Validation", "Failed to validate apartment deletion eligibility", ex);
        }
    }

    private async Task ValidateNoActiveBookingsAsync(int apartmentId)
    {
        var activeBookings = await _bookingsRepository.GetActiveAndFutureBookingsAsync(apartmentId);

        if (activeBookings.Any())
        {
            var bookingDetails = activeBookings
                .Select(b => $"Booking #{b.Id}: {b.DateFrom:yyyy-MM-dd} to {b.DateTo:yyyy-MM-dd} (User: {b.User.Username})")
                .ToList();

            throw new BusinessRuleViolationException(
                "APARTMENT_HAS_ACTIVE_BOOKINGS",
                $"Cannot delete apartment with {activeBookings.Count()} active or future booking(s). Please wait until all bookings are completed or cancel them first.");
        }
    }

    private async Task ProcessApartmentImagesAsync(int apartmentId, IEnumerable<ImageDTO> photos)
    {
        if (photos?.Any() != true) return;

        Action<Image> setApartmentIdDelegate = image => image.EstablishmentId = apartmentId;

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
            var existingImageIds = (await _imagesRepository.GetApartmentImagesAsync(apartmentId))
                .Select(image => image.Id)
                .ToList();

            if (existingImageIds.Any())
            {
                await _imagesService.DeleteImagesAsync(existingImageIds, BlobContainerName);
            }
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("ImageProcessing", "Failed to remove apartment images", ex);
        }
    }
}