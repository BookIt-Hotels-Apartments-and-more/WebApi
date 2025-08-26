using BookIt.BLL.DTOs;
using BookIt.DAL.Enums;

namespace BookIt.BLL.Interfaces;

public interface IUserService
{
    Task<UserAuthDTO> RegisterAsync(string username, string email, string? password, UserRole role = UserRole.Tenant, string? imageUrl = null);
    Task<bool> IsAdmin(int userId);
    Task<IEnumerable<UserDTO>> GetUsersAsync();
    Task<UserAuthDTO> LoginAsync(string email, string password);
    Task<UserDTO?> GetUserByIdAsync(int id);
    Task<UserAuthDTO?> AuthByGoogleAsync(string username, string email, string? imageUrl, UserRole role = UserRole.Tenant);
    Task<UserDTO?> VerifyEmailAsync(string token);
    Task<UserDTO?> GenerateResetPasswordTokenAsync(string email);
    Task<UserDTO?> ResetPasswordAsync(string token, string newPassword);
    Task ChangeUserPasswordAsync(int userId, string currentPassword, string newPassword);
    Task<IEnumerable<UserDTO>> GetAllUsersAsync(UserRole? role = null);
    Task ChangeUserRoleAsync(int userId, UserRole role);
}
