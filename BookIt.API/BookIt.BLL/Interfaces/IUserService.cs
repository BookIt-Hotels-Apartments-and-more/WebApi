using BookIt.DAL.Models;

namespace BookIt.BLL.Services;

public interface IUserService
{
    Task<User> RegisterAsync(string username, string email, string password, UserRole role);
    Task<bool> IsAdmin(int userId);
    Task<List<User>> GetUsersAsync();
    Task<User?> LoginAsync(string email, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> AuthByGoogleAsync(string username, string email);
    Task<User?> VerifyEmailAsync(string token);
    Task<User?> GenerateResetPasswordTokenAsync(string email);
    Task<User?> ResetPasswordAsync(string token, string newPassword);
    Task<IEnumerable<User>> GetAllUsersAsync(UserRole? role = null);

}
