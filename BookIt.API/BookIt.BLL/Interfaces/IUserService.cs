using BookIt.DAL.Models;

namespace BookIt.BLL.Services;

public interface IUserService
{
    Task<User> RegisterAsync(string username, string email, string password, UserRole role);
    Task<User?> LoginAsync(string email, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> AuthByGoogleAsync(string username, string email);
    Task<User?> VerifyEmailAsync(string token);
}
