using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class ImagesService : IImagesService
{
    private readonly IMapper _mapper;
    private readonly ImagesRepository _repository;
    private readonly IBlobStorageService _blobStorageService;

    public ImagesService(IMapper mapper, ImagesRepository repository, IBlobStorageService blobStorageService)
    {
        _mapper = mapper;
        _repository = repository;
        _blobStorageService = blobStorageService;
    }

    public async Task<List<ImageDTO>> SaveImagesAsync(List<ImageDTO> images, string blobContainerName, Action<Image> parentEntityIdSetter)
    {
        List<Image> addedImages = new List<Image>();

        foreach (var image in images)
        {
            if ((image.Id is not null && await _repository.ExistsAsync(image.Id.Value)) ||
                (image.Base64Image is null))
                continue;

            var randomFileName = Guid.NewGuid().ToString() + ".jpeg";
            var blobUrl = await _blobStorageService.UploadImageAsync(image.Base64Image, blobContainerName, randomFileName);

            var imageDomain = new Image { BlobUrl = blobUrl };
            parentEntityIdSetter(imageDomain);

            var newImage = await _repository.AddAsync(imageDomain);
            addedImages.Add(newImage);
        }

        var addedImagesDto = _mapper.Map<List<ImageDTO>>(addedImages);
        return addedImagesDto;
    }

    public async Task<bool> DeleteImagesAsync(List<int> ids, string blobContainerName)
    {
        foreach(var id in ids)
        {
            var image = await _repository.GetByIdAsync(id);
            if (image is null)
                continue;

            var isDeleted = await _blobStorageService.DeleteImageAsync(image.BlobUrl, blobContainerName);
            if (!isDeleted)
                return false;

            await _repository.DeleteAsync(id);
        }

        return true;
    }
}
