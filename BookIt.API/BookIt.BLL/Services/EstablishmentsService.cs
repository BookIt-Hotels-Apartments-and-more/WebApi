using BookIt.DAL.Models;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Repositories;
using BookIt.BLL.DTOs;
using AutoMapper;
using System.Linq.Expressions;
using BookIt.DAL.Enums;

namespace BookIt.BLL.Services;

public class EstablishmentsService : IEstablishmentsService
{
    private const string BlobContainerName = "establishments";

    private readonly IMapper _mapper;
    private readonly IImagesService _imagesService;
    private readonly IRatingsService _ratingsService;
    private readonly ImagesRepository _imagesRepository;
    private readonly IGeolocationService _geolocationService;
    private readonly IClassificationService _classificationService;
    private readonly EstablishmentsRepository _establishmentsRepository;

    public EstablishmentsService(
        IMapper mapper,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ImagesRepository imagesRepository,
        IGeolocationService geolocationService,
        IClassificationService classificationService,
        EstablishmentsRepository establishmentsRepository)
    {
        _mapper = mapper;
        _imagesService = imagesService;
        _ratingsService = ratingsService;
        _imagesRepository = imagesRepository;
        _geolocationService = geolocationService;
        _classificationService = classificationService;
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<IEnumerable<EstablishmentDTO>> GetAllAsync()
    {
        var establishmentsDomain = await _establishmentsRepository.GetAllAsync();
        var establishmentsDto = _mapper.Map<IEnumerable<EstablishmentDTO>>(establishmentsDomain);
        return establishmentsDto;
    }

    public async Task<EstablishmentDTO?> GetByIdAsync(int id)
    {
        var establishmentDomain = await _establishmentsRepository.GetByIdAsync(id);
        if (establishmentDomain is null) return null;
        var establishmentDto = _mapper.Map<EstablishmentDTO>(establishmentDomain);
        return establishmentDto;
    }

    public async Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto)
    {
        Task<GeolocationDTO?> addGeolocationTask = _geolocationService.CreateAsync(dto.Geolocation);
        Task<VibeType?> classifyVibeTask = _classificationService.ClassifyEstablishmentVibeAsync(dto);

        await Task.WhenAll(addGeolocationTask, classifyVibeTask);

        if (addGeolocationTask.Result?.Id is null)
            return null;

        var establishmentDomain = _mapper.Map<Establishment>(dto);

        establishmentDomain.GeolocationId = addGeolocationTask.Result.Id;
        establishmentDomain.Vibe = classifyVibeTask.Result ?? VibeType.None;

        var addedEstablishment = await _establishmentsRepository.AddAsync(establishmentDomain);

        Action<Image> setEstablishmentIdDelegate = image => image.EstablishmentId = addedEstablishment.Id;

        await _imagesService.SaveImagesAsync(dto.Photos, BlobContainerName, setEstablishmentIdDelegate);
        
        return await GetByIdAsync(addedEstablishment.Id);
    }

    public async Task<EstablishmentDTO?> UpdateAsync(int id, EstablishmentDTO dto)
    {
        var establishmentExists = await _establishmentsRepository.ExistsAsync(id);
        if (!establishmentExists) return null;
        var establishmentDomain = _mapper.Map<Establishment>(dto);
        establishmentDomain.Id = id;

        Task<GeolocationDTO?> updateGeolocationTask = _geolocationService.UpdateEstablishmentGeolocationAsync(id, dto.Geolocation);
        Task<VibeType?> updateVibeTask = _classificationService.UpdateEstablishmentVibeAsync(id, dto);

        await Task.WhenAll(updateGeolocationTask, updateVibeTask);

        if (updateGeolocationTask.Result?.Id is not null)
            establishmentDomain.GeolocationId = updateGeolocationTask.Result?.Id;

        if (updateVibeTask.Result is not null)
            establishmentDomain.Vibe = updateVibeTask.Result;

        await _establishmentsRepository.UpdateAsync(establishmentDomain);

        Action<Image> setEstablishmentIdDelegate = image => image.EstablishmentId = id;

        var idsOfExistingPhotosForEstablishment = (await _imagesRepository
            .GetEstablishmentImagesAsync(id))
            .Select(photo => photo.Id)
            .ToList();

        var idsOfPhotosToKeep = dto.Photos
            .Where(photo => photo.Id is not null && photo.Base64Image is null)
            .Select(photo => photo.Id!.Value)
            .ToList();

        var idsOfPhotosToRemove = idsOfExistingPhotosForEstablishment
            .Where(id => !idsOfPhotosToKeep.Contains(id))
            .ToList();

        var photosToAdd = dto.Photos
            .Where(photo => photo.Id is null && photo.Base64Image is not null)
            .ToList();

        await Task.WhenAll(_imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName),
                           _imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setEstablishmentIdDelegate));

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var establishmentExists = await _establishmentsRepository.ExistsAsync(id);
        if (!establishmentExists) return false;

        var idsOfEstablishmentImages = (await _imagesRepository
            .GetEstablishmentImagesAsync(id))
            .Select(image => image.Id)
            .ToList();

        await _imagesService.DeleteImagesAsync(idsOfEstablishmentImages, BlobContainerName);

        await _geolocationService.DeleteEstablishmentGeolocationAsync(id);

        await _establishmentsRepository.DeleteAsync(id);

        return true;
    }

    public List<string> GetFeatureList(Establishment establishment)
    {
        return Enum.GetValues<EstablishmentFeatures>()
            .Where(f => f != EstablishmentFeatures.None && establishment.Features.HasFlag(f))
            .Select(f => f.ToString())
            .ToList();
    }

    public async Task<PagedResultDTO<EstablishmentDTO>> GetFilteredAsync(EstablishmentFilterDTO filter)
    {
        Expression<Func<Establishment, bool>> predicate = e => true;

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            predicate = predicate.And(e => e.Name.Contains(filter.Name));
        }

        if (filter.Type.HasValue)
        {
            predicate = predicate.And(e => e.Type == filter.Type.Value);
        }

        if (filter.Features.HasValue)
        {
            predicate = predicate.And(e => (e.Features & filter.Features.Value) == filter.Features.Value);
        }

        if (filter.OwnerId.HasValue)
        {
            predicate = predicate.And(e => e.OwnerId == filter.OwnerId.Value);
        }

        var (establishments, totalCount) = await _establishmentsRepository.GetFilteredAsync(
            predicate,
            filter.Page,
            filter.PageSize);

        var establishmentsDto = _mapper.Map<IEnumerable<EstablishmentDTO>>(establishments);

        var filteredEstablishments = establishmentsDto.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            filteredEstablishments = filteredEstablishments.Where(e =>
                e.Geolocation.Country != null &&
                e.Geolocation.Country.Contains(filter.Country, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            filteredEstablishments = filteredEstablishments.Where(e =>
                e.Geolocation.City != null &&
                e.Geolocation.City.Contains(filter.City, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.MinRating.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Rating?.GeneralRating >= filter.MinRating.Value);
        }

        if (filter.MaxRating.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Rating?.GeneralRating <= filter.MaxRating.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Price <= filter.MaxPrice.Value);
        }

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
}
