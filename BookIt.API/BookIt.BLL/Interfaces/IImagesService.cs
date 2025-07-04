using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.BLL.Interfaces;

public interface IImagesService
{
    Task<List<ImageDTO>> SaveImagesAsync(List<ImageDTO> images, string blobContainerName, Action<Image> parentEntityIdSetter);
    Task<bool> DeleteImagesAsync(List<int> ids, string blobContainerName);
}
