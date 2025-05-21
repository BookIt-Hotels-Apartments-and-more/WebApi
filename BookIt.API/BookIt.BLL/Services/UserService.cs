using BookIt.DAL.Repositories;
using BookIt.DAL.Models;
using System.Security.Cryptography;
using System.Text;

namespace BookIt.BLL.Services;

public class UserService : IUserService
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

        var token = Guid.NewGuid().ToString();

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmationToken = token
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<User?> AuthByGoogleAsync(string username, string email)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(email);

        if (existingUser)
        {
            return await _userRepository.GetByEmailAsync(email);
        }
        else
        {
            return await RegisterAsync(username, email, "", UserRole.Tenant);
        }
    }

    public async Task<User?> VerifyEmailAsync(string token)
    {
        User? user = await _userRepository.GetByEmailTokenAsync(token) ?? throw new Exception("Invalid email token");

        user.EmailConfirmationToken = null;
        user.IsEmailConfirmed = true;

        await _userRepository.UpdateAsync(user);

        return user;
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
