using BookIt.DAL.Repositories;
using BookIt.DAL.Models;
using System.Security.Cryptography;
using System.Text;

namespace BookIt.BLL.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> RegisterAsync(string username, string email, string password, UserRole role)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(email);
        if (existingUser)
        {
            throw new Exception("User existing");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<User> AuthByGoogleAsync(string username, string email)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(email);

        if (!existingUser)
        {
            return await _userRepository.GetByEmailAsync(email);
        }
        else
        {
            return await RegisterAsync(username, email, null, UserRole.Tenant);
        }
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var hashedPassword = HashPassword(password);
        return await _userRepository.GetByEmailAndPasswordHashAsync(email, hashedPassword);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
