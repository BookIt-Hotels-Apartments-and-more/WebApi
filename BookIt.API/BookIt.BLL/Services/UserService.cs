using BookIt.DAL.Repositories;
using BookIt.DAL.Models;
using System.Security.Cryptography;
using System.Text;
using BookIt.DAL.Enums;
using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;

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
        try
        {
            var existingUser = await _userRepository.ExistsByEmailAsync(email);
            if (existingUser)
            {
                throw new EntityAlreadyExistsException("User", "email", email);
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
            return _mapper.Map<UserDTO>(registeredUser);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to register user", ex);
        }
    }

    public async Task<bool> IsAdmin(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new EntityNotFoundException("User", userId);
            }

            return user.Role == UserRole.Admin;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to check user admin status", ex);
        }
    }

    public async Task<IEnumerable<UserDTO>> GetUsersAsync()
    {
        try
        {
            var usersDomain = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(usersDomain);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve users", ex);
        }
    }

    public async Task<UserDTO?> AuthByGoogleAsync(string username, string email)
    {
        try
        {
            var existingUser = await _userRepository.ExistsByEmailAsync(email);

            if (existingUser)
            {
                var existingUserDomain = await _userRepository.GetByEmailAsync(email);
                return _mapper.Map<UserDTO>(existingUserDomain);
            }
            else
            {
                return await RegisterAsync(username, email, null, UserRole.Tenant);
            }
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Google Auth", "Failed to authenticate with Google", ex);
        }
    }

    public async Task<UserDTO?> VerifyEmailAsync(string token)
    {
        try
        {
            var userDomain = await _userRepository.GetByEmailTokenAsync(token);
            if (userDomain is null)
            {
                throw new BusinessRuleViolationException("INVALID_TOKEN", "Invalid or expired email verification token");
            }

            userDomain.EmailConfirmationToken = null;
            userDomain.IsEmailConfirmed = true;

            await _userRepository.UpdateAsync(userDomain);
            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to verify email", ex);
        }
    }

    public async Task<UserDTO?> GenerateResetPasswordTokenAsync(string email)
    {
        try
        {
            var userDomain = await _userRepository.GetByEmailAsync(email);
            if (userDomain is null)
            {
                throw new EntityNotFoundException("User", $"email '{email}'");
            }

            var token = Guid.NewGuid().ToString();
            userDomain.ResetPasswordToken = token;

            await _userRepository.UpdateAsync(userDomain);
            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to generate reset password token", ex);
        }
    }

    public async Task<UserDTO?> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var userDomain = await _userRepository.GetByResetPasswordTokenAsync(token);
            if (userDomain is null)
            {
                throw new BusinessRuleViolationException("INVALID_TOKEN", "Invalid or expired reset password token");
            }

            userDomain.ResetPasswordToken = null;
            userDomain.PasswordHash = HashPassword(newPassword);

            await _userRepository.UpdateAsync(userDomain);
            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to reset password", ex);
        }
    }

    public async Task<UserDTO?> LoginAsync(string email, string password)
    {
        try
        {
            var hashedPassword = HashPassword(password);
            var userDomain = await _userRepository.GetByEmailAndPasswordHashAsync(email, hashedPassword);

            if (userDomain is null)
            {
                throw new UnauthorizedOperationException("Invalid email or password");
            }

            //if (!userDomain.IsEmailConfirmed)
            //{
            //    throw new BusinessRuleViolationException("EMAIL_NOT_CONFIRMED", "Please confirm your email before logging in");
            //}

            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to authenticate user", ex);
        }
    }

    public async Task<UserDTO?> GetUserByIdAsync(int id)
    {
        try
        {
            var userDomain = await _userRepository.GetByIdAsync(id);
            if (userDomain is null)
            {
                throw new EntityNotFoundException("User", id);
            }

            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve user", ex);
        }
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(UserRole? role = null)
    {
        try
        {
            var usersDomain = role.HasValue
                ? await _userRepository.GetAllByRoleAsync(role.Value)
                : await _userRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<UserDTO>>(usersDomain);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve users", ex);
        }
    }

    private string HashPassword(string password)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Cryptography", "Failed to hash password", ex);
        }
    }
}