using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IUserManagementService
{
    Task SetUserImagesAsync(int userId, IEnumerable<ImageDTO> images);
}
