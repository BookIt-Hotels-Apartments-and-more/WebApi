using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Enums;
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
    private readonly ApartmentsRepository _apartmentsRepository;

    public ApartmentsService(IMapper mapper,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ImagesRepository imagesRepository,
        ApartmentsRepository apartmentsRepository)
    {
        _mapper = mapper;
        _imagesService = imagesService;
        _ratingsService = ratingsService;
        _imagesRepository = imagesRepository;
        _apartmentsRepository = apartmentsRepository;
    }

    public async Task<IEnumerable<ApartmentDTO>> GetAllAsync()
    {
        var apartmentsDomain = await _apartmentsRepository.GetAllAsync();
        var apartmentsDto = _mapper.Map<IEnumerable<ApartmentDTO>>(apartmentsDomain);
        return apartmentsDto;
    }

    public async Task<ApartmentDTO?> GetByIdAsync(int id)
    {
        var apartmentDomain = await _apartmentsRepository.GetByIdAsync(id);
        if (apartmentDomain is null) return null;
        var apartmentDto = _mapper.Map<ApartmentDTO>(apartmentDomain);
        return apartmentDto;
    }

    public async Task<ApartmentDTO?> CreateAsync(ApartmentDTO dto)
    {
        var apartmentDomain = _mapper.Map<Apartment>(dto);
        var addedApartment = await _apartmentsRepository.AddAsync(apartmentDomain);

        Action<Image> setApartmentIdDelegate = image => image.ApartmentId = addedApartment.Id;

        await _imagesService.SaveImagesAsync(dto.Photos, BlobContainerName, setApartmentIdDelegate);

        return await GetByIdAsync(addedApartment.Id);
    }

    public async Task<ApartmentDTO?> UpdateAsync(int id, ApartmentDTO dto)
    {
        var apartmentExists = await _apartmentsRepository.ExistsAsync(id);
        if (!apartmentExists) return null;
        var apartmentDomain = _mapper.Map<Apartment>(dto);
        apartmentDomain.Id = id;
        await _apartmentsRepository.UpdateAsync(apartmentDomain);

        Action<Image> setApartmentIdDelegate = image => image.ApartmentId = id;

        var idsOfExistingPhotosForApartment = (await _imagesRepository
            .GetApartmentImagesAsync(id))
            .Select(photo => photo.Id)
            .ToList();

        var idsOfPhotosToKeep = dto.Photos
            .Where(photo => photo.Id is not null && photo.Base64Image is null)
            .Select(photo => photo.Id!.Value)
            .ToList();

        var idsOfPhotosToRemove = idsOfExistingPhotosForApartment
            .Where(id => !idsOfPhotosToKeep.Contains(id))
            .ToList();

        await _imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName);

        var photosToAdd = dto.Photos
            .Where(photo => photo.Id is null && photo.Base64Image is not null)
            .ToList();

        await _imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setApartmentIdDelegate);

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var apartmentExists = await _apartmentsRepository.ExistsAsync(id);
        if (!apartmentExists) return false;

        var idsOfApartmentImages = (await _imagesRepository
            .GetApartmentImagesAsync(id))
            .Select(image => image.Id)
            .ToList();

        await _imagesService.DeleteImagesAsync(idsOfApartmentImages, BlobContainerName);

        await _apartmentsRepository.DeleteAsync(id);

        return true;
    }

    public List<string> GetFeatureList(Apartment apartment)
    {
        return Enum.GetValues<ApartmentFeatures>()
            .Where(f => f != ApartmentFeatures.None && apartment.Features.HasFlag(f))
            .Select(f => f.ToString())
            .ToList();
    }

    public async Task<PagedResultDTO<ApartmentDTO>> GetPagedByEstablishmentIdAsync(int establishmentId, int page, int pageSize)
    {
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
}
