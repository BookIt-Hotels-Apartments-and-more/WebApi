using BookIt.DAL.Models;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Repositories;
using BookIt.BLL.DTOs;
using AutoMapper;

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
}
