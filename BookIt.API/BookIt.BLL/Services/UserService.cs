using BookIt.DAL.Repositories;
using BookIt.DAL.Models;
using System.Security.Cryptography;
using System.Text;
using BookIt.DAL.Enums;
using AutoMapper;
using BookIt.BLL.DTOs;

namespace BookIt.BLL.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;

    public UserService(IMapper mapper, UserRepository userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<UserDTO> RegisterAsync(string username, string email, string? password, UserRole role)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(email);
        if (existingUser)
        {
            throw new Exception("User exists");
        }

        var token = Guid.NewGuid().ToString();

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = string.IsNullOrEmpty(password) ? password : HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmationToken = token
        };

        var registeredUser = await _userRepository.CreateAsync(user);
        var userDto = _mapper.Map<UserDTO>(registeredUser);
        return userDto;
    }
    
    public async Task<bool> IsAdmin(int userId)
    {

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new Exception("User not found");
        }

        return user.Role == UserRole.Admin;
    }

    public async Task<IEnumerable<UserDTO>> GetUsersAsync()
    {
        var usersDomain = await _userRepository.GetAllAsync();
        var usersDto = _mapper.Map<IEnumerable<UserDTO>>(usersDomain);
        return usersDto;
    }

    public async Task<UserDTO?> AuthByGoogleAsync(string username, string email)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(email);

        if (existingUser)
        {
            var existingUserDomain = await _userRepository.GetByEmailAsync(email);
            var userDto = _mapper.Map<UserDTO>(existingUserDomain);
            return userDto;
        }
        else
        {
            var registeredUser = await RegisterAsync(username, email, null, UserRole.Tenant);
            var userDto = _mapper.Map<UserDTO>(registeredUser);
            return userDto;
        }
    }

    public async Task<UserDTO?> VerifyEmailAsync(string token)
    {
        var userDomain = await _userRepository.GetByEmailTokenAsync(token) ?? throw new Exception("Invalid email token");

        userDomain.EmailConfirmationToken = null;
        userDomain.IsEmailConfirmed = true;

        await _userRepository.UpdateAsync(userDomain);

        var userDto = _mapper.Map<UserDTO>(userDomain);
        return userDto;
    }

    public async Task<UserDTO?> GenerateResetPasswordTokenAsync(string email)
    {
        var token = Guid.NewGuid().ToString();
        var userDomain = await _userRepository.GetByEmailAsync(email) ?? throw new Exception("Invalid email");

        userDomain.ResetPasswordToken = token;

        await _userRepository.UpdateAsync(userDomain);

        var userDto = _mapper.Map<UserDTO>(userDomain);
        return userDto;
    }

    public async Task<UserDTO?> ResetPasswordAsync(string token, string newPassword)
    {
        var userDomain = await _userRepository.GetByResetPasswordTokenAsync(token) ?? throw new Exception("Invalid email token");

        userDomain.ResetPasswordToken = null;
        userDomain.PasswordHash = HashPassword(newPassword);

        await _userRepository.UpdateAsync(userDomain);

        var userDto = _mapper.Map<UserDTO>(userDomain);
        return userDto;
    }

    public async Task<UserDTO?> LoginAsync(string email, string password)
    {
        var hashedPassword = HashPassword(password);
        var userDomain = await _userRepository.GetByEmailAndPasswordHashAsync(email, hashedPassword);
        var userDto = _mapper.Map<UserDTO>(userDomain);
        return userDto;
    }

    public async Task<UserDTO?> GetUserByIdAsync(int id)
    {
        var userDomain = await _userRepository.GetByIdAsync(id);
        var userDto = _mapper.Map<UserDTO>(userDomain);
        return userDto;
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(UserRole? role = null)
    {
        var usersDomain = role.HasValue
            ? await _userRepository.GetAllByRoleAsync(role.Value)
            : await _userRepository.GetAllAsync();

        var usersDto = _mapper.Map<IEnumerable<UserDTO>>(usersDomain);
        return usersDto;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
