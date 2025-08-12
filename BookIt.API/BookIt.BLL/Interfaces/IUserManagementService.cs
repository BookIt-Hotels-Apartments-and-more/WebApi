using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IUserManagementService
{
    Task SetUserImagesAsync(int userId, IEnumerable<ImageDTO> images);
    Task<IEnumerable<ImageDTO>> GetUserImagesAsync(int userId);
    Task<bool> DeleteAllUserImagesAsync(int userId);
    Task UpdateUserDetailsAsync(UserDetailsDTO dto);
}
