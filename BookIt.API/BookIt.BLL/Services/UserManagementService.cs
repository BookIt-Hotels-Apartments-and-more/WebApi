using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class UserManagementService : IUserManagementService
{
    private const string BlobContainerName = "users";

    private readonly IImagesService _imagesService;
    private readonly ImagesRepository _imagesRepository;

    public UserManagementService(IImagesService imagesService, ImagesRepository imagesRepository)
    {
        _imagesService = imagesService;
        _imagesRepository = imagesRepository;
    }

    public async Task SetUserImagesAsync(int userId, IEnumerable<ImageDTO> images)
    {
        Action<Image> setUserIdDelegate = image => image.UserId = userId;

        var idsOfExistingPhotosForUser = (await _imagesRepository
            .GetUserImagesAsync(userId))
            .Select(photo => photo.Id)
            .ToList();

        var idsOfPhotosToKeep = images
            .Where(photo => photo.Id is not null && photo.Base64Image is null)
            .Select(photo => photo.Id!.Value)
            .ToList();

        var idsOfPhotosToRemove = idsOfExistingPhotosForUser
            .Where(id => !idsOfPhotosToKeep.Contains(id))
            .ToList();

        await _imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName);

        var photosToAdd = images
            .Where(photo => photo.Id is null && photo.Base64Image is not null)
            .ToList();

        await _imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setUserIdDelegate);
    }
}
