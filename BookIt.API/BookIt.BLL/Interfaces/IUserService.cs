using BookIt.BLL.DTOs;
using BookIt.DAL.Enums;

namespace BookIt.BLL.Interfaces;

public interface IUserService
{
    Task<UserDTO> RegisterAsync(string username, string email, string? password, UserRole role);
    Task<bool> IsAdmin(int userId);
    Task<IEnumerable<UserDTO>> GetUsersAsync();
    Task<UserDTO?> LoginAsync(string email, string password);
    Task<UserDTO?> GetUserByIdAsync(int id);
    Task<UserDTO?> AuthByGoogleAsync(string username, string email);
    Task<UserDTO?> VerifyEmailAsync(string token);
    Task<UserDTO?> GenerateResetPasswordTokenAsync(string email);
    Task<UserDTO?> ResetPasswordAsync(string token, string newPassword);
    Task ChangeUserPasswordAsync(int userId, string currentPassword, string newPassword);
    Task<IEnumerable<UserDTO>> GetAllUsersAsync(UserRole? role = null);
    Task ChangeUserRoleAsync(int userId, UserRole role);
}
