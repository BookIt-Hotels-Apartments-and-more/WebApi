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
    private readonly EstablishmentsRepository _establishmentsRepository;

    public EstablishmentsService(
        IMapper mapper,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ImagesRepository imagesRepository,
        IGeolocationService geolocationService,
        EstablishmentsRepository establishmentsRepository)
    {
        _mapper = mapper;
        _imagesService = imagesService;
        _ratingsService = ratingsService;
        _geolocationService = geolocationService;
        _imagesRepository = imagesRepository;
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<IEnumerable<EstablishmentDTO>> GetAllAsync()
    {
        var establishmentsDomain = await _establishmentsRepository.GetAllAsync();
        var establishmentsDto = _mapper.Map<IEnumerable<EstablishmentDTO>>(establishmentsDomain);

        foreach (var establishment in establishmentsDto)
            establishment.Rating = await _ratingsService.CalculateRating(establishment);

        return establishmentsDto;
    }

    public async Task<EstablishmentDTO?> GetByIdAsync(int id)
    {
        var establishmentDomain = await _establishmentsRepository.GetByIdAsync(id);
        if (establishmentDomain is null) return null;
        var establishmentDto = _mapper.Map<EstablishmentDTO>(establishmentDomain);
        establishmentDto.Rating = await _ratingsService.CalculateRating(establishmentDto);
        return establishmentDto;
    }

    public async Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto)
    {
        var addedGeolocationDto = await _geolocationService.CreateAsync(dto.Geolocation);
        if (addedGeolocationDto is null || addedGeolocationDto.Id is null)
            return null;

        var establishmentDomain = _mapper.Map<Establishment>(dto);
        establishmentDomain.GeolocationId = addedGeolocationDto.Id.Value;

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

        var geolocation = await _geolocationService.UpdateEstablishmentGeolocationAsync(id, dto.Geolocation);

        if (geolocation?.Id is not null)
            establishmentDomain.GeolocationId = geolocation.Id;

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

        await _imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName);

        var photosToAdd = dto.Photos
            .Where(photo => photo.Id is null && photo.Base64Image is not null)
            .ToList();

        await _imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setEstablishmentIdDelegate);

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

        // Calculate rating for each establishment
        foreach (var establishment in establishmentsDto)
        {
            establishment.Rating = await _ratingsService.CalculateRating(establishment);
        }

        // Apply post-database filtering for fields that need to be filtered in-memory
        var filteredEstablishments = establishmentsDto.AsEnumerable();

        // Filter by country and city (these are in the geolocation)
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

        // Filter by rating
        if (filter.MinRating.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Rating >= filter.MinRating.Value);
        }

        if (filter.MaxRating.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Rating <= filter.MaxRating.Value);
        }

        // Filter by price
        if (filter.MinPrice.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            filteredEstablishments = filteredEstablishments.Where(e => e.Price <= filter.MaxPrice.Value);
        }

        // Count after in-memory filtering
        var finalCount = filteredEstablishments.Count();

        // Calculate pagination metadata
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

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }
}
