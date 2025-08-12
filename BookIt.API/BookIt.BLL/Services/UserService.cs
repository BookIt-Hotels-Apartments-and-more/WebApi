using BookIt.DAL.Repositories;
using BookIt.DAL.Models;
using System.Security.Cryptography;
using System.Text;
using BookIt.DAL.Enums;
using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BookIt.BLL.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IMapper mapper, UserRepository userRepository, ILogger<UserService> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDTO> RegisterAsync(string username, string email, string? password, UserRole role)
    {
        _logger.LogInformation("RegisterAsync started for Email={Email}, Username={Username}, Role={Role}", email, username, role);
        try
        {
            var existingUser = await _userRepository.ExistsByEmailAsync(email);
            if (existingUser)
            {
                _logger.LogWarning("RegisterAsync failed: user with Email={Email} already exists", email);
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
            _logger.LogInformation("User registered successfully with Id={UserId}", registeredUser.Id);
            return _mapper.Map<UserDTO>(registeredUser);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogInformation(ex, "SQL error while registering user {UserId}. Reason: {Message}", username, ex.InnerException?.Message);

            var duplicateProperties = new List<string>();

            if (ex.InnerException?.Message?.Contains($"The duplicate key value is ({email})") ?? false)
                duplicateProperties.Add(nameof(email));

            if (ex.InnerException?.Message?.Contains($"The duplicate key value is ({username})") ?? false)
                duplicateProperties.Add(nameof(username));

            throw new BusinessRuleViolationException(
                "USER_DETAILS_ARE_NOT_UNIQUE",
                "Some of user details should be unique",
                 new Dictionary<string, object>
                 { { "DuplicateProperties", duplicateProperties }});
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user with Email={Email}", email);
            throw new ExternalServiceException("Database", "Failed to register user", ex);
        }
    }

    public async Task<bool> IsAdmin(int userId)
    {
        _logger.LogInformation("IsAdmin check started for UserId={UserId}", userId);
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("IsAdmin check failed: user with Id={UserId} not found", userId);
                throw new EntityNotFoundException("User", userId);
            }

            var isAdmin = user.Role == UserRole.Admin;
            _logger.LogInformation("IsAdmin check for UserId={UserId} result: {IsAdmin}", userId, isAdmin);
            return isAdmin;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check admin status for UserId={UserId}", userId);
            throw new ExternalServiceException("Database", "Failed to check user admin status", ex);
        }
    }

    public async Task<IEnumerable<UserDTO>> GetUsersAsync()
    {
        _logger.LogInformation("GetUsersAsync started");
        try
        {
            var usersDomain = await _userRepository.GetAllAsync();
            _logger.LogInformation("GetUsersAsync succeeded, retrieved {Count} users", usersDomain.Count());
            return _mapper.Map<IEnumerable<UserDTO>>(usersDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users");
            throw new ExternalServiceException("Database", "Failed to retrieve users", ex);
        }
    }

    public async Task<UserDTO?> AuthByGoogleAsync(string username, string email)
    {
        _logger.LogInformation("AuthByGoogleAsync started for Email={Email}, Username={Username}", email, username);
        try
        {
            var existingUser = await _userRepository.ExistsByEmailAsync(email);

            if (existingUser)
            {
                var existingUserDomain = await _userRepository.GetByEmailAsync(email);
                _logger.LogInformation("AuthByGoogleAsync: existing user found for Email={Email}", email);
                return _mapper.Map<UserDTO>(existingUserDomain);
            }
            else
            {
                _logger.LogInformation("AuthByGoogleAsync: no existing user, registering new user for Email={Email}", email);
                return await RegisterAsync(username, email, null, UserRole.Tenant);
            }
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with Google for Email={Email}", email);
            throw new ExternalServiceException("Google Auth", "Failed to authenticate with Google", ex);
        }
    }

    public async Task<UserDTO?> VerifyEmailAsync(string token)
    {
        _logger.LogInformation("VerifyEmailAsync started for Token={Token}", token);
        try
        {
            var userDomain = await _userRepository.GetByEmailTokenAsync(token);
            if (userDomain is null)
            {
                _logger.LogWarning("VerifyEmailAsync failed: invalid or expired token");
                throw new BusinessRuleViolationException("INVALID_TOKEN", "Invalid or expired email verification token");
            }

            userDomain.EmailConfirmationToken = null;
            userDomain.IsEmailConfirmed = true;

            await _userRepository.UpdateAsync(userDomain);
            _logger.LogInformation("VerifyEmailAsync succeeded for UserId={UserId}", userDomain.Id);
            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify email for Token={Token}", token);
            throw new ExternalServiceException("Database", "Failed to verify email", ex);
        }
    }

    public async Task<UserDTO?> GenerateResetPasswordTokenAsync(string email)
    {
        _logger.LogInformation("GenerateResetPasswordTokenAsync started for Email={Email}", email);
        try
        {
            var userDomain = await _userRepository.GetByEmailAsync(email);
            if (userDomain is null)
            {
                _logger.LogWarning("GenerateResetPasswordTokenAsync failed: user with Email={Email} not found", email);
                throw new EntityNotFoundException("User", $"email '{email}'");
            }

            var token = Guid.NewGuid().ToString();
            userDomain.ResetPasswordToken = token;

            await _userRepository.UpdateAsync(userDomain);
            _logger.LogInformation("Reset password token generated for UserId={UserId}", userDomain.Id);
            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate reset password token for Email={Email}", email);
            throw new ExternalServiceException("Database", "Failed to generate reset password token", ex);
        }
    }

    public async Task<UserDTO?> ResetPasswordAsync(string token, string newPassword)
    {
        _logger.LogInformation("ResetPasswordAsync started for Token={Token}", token);
        try
        {
            var userDomain = await _userRepository.GetByResetPasswordTokenAsync(token);
            if (userDomain is null)
            {
                _logger.LogWarning("ResetPasswordAsync failed: invalid or expired token");
                throw new BusinessRuleViolationException("INVALID_TOKEN", "Invalid or expired reset password token");
            }

            userDomain.ResetPasswordToken = null;
            userDomain.PasswordHash = HashPassword(newPassword);

            await _userRepository.UpdateAsync(userDomain);
            _logger.LogInformation("Password reset successfully for UserId={UserId}", userDomain.Id);
            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset password for Token={Token}", token);
            throw new ExternalServiceException("Database", "Failed to reset password", ex);
        }
    }

    public async Task<UserDTO?> LoginAsync(string email, string password)
    {
        _logger.LogInformation("LoginAsync started for Email={Email}", email);
        try
        {
            var hashedPassword = HashPassword(password);
            var userDomain = await _userRepository.GetByEmailAndPasswordHashAsync(email, hashedPassword);

            if (userDomain is null)
            {
                _logger.LogWarning("LoginAsync failed: invalid email or password for Email={Email}", email);
                throw new UnauthorizedOperationException("Invalid email or password");
            }

            _logger.LogInformation("LoginAsync succeeded for UserId={UserId}", userDomain.Id);
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
            _logger.LogError(ex, "Failed to authenticate user for Email={Email}", email);
            throw new ExternalServiceException("Database", "Failed to authenticate user", ex);
        }
    }

    public async Task<UserDTO?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("GetUserByIdAsync started for UserId={UserId}", id);
        try
        {
            var userDomain = await _userRepository.GetByIdAsync(id);
            if (userDomain is null)
            {
                _logger.LogWarning("GetUserByIdAsync failed: user with Id={UserId} not found", id);
                throw new EntityNotFoundException("User", id);
            }

            _logger.LogInformation("GetUserByIdAsync succeeded for UserId={UserId}", id);
            return _mapper.Map<UserDTO>(userDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user for UserId={UserId}", id);
            throw new ExternalServiceException("Database", "Failed to retrieve user", ex);
        }
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(UserRole? role = null)
    {
        _logger.LogInformation("GetAllUsersAsync started with Role filter: {Role}", role?.ToString() ?? "None");
        try
        {
            var usersDomain = role.HasValue
                ? await _userRepository.GetAllByRoleAsync(role.Value)
                : await _userRepository.GetAllAsync();

            _logger.LogInformation("GetAllUsersAsync succeeded, retrieved {Count} users", usersDomain.Count());
            return _mapper.Map<IEnumerable<UserDTO>>(usersDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users");
            throw new ExternalServiceException("Database", "Failed to retrieve users", ex);
        }
    }

    public async Task ChangeUserRoleAsync(int userId, UserRole role)
    {
        _logger.LogInformation("Changing user {UserId} role to {Role}", userId, role);
        try
        {
            var currentUser = await _userRepository.GetByIdAsync(userId);
            if (currentUser is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw new EntityNotFoundException("User", userId);
            }

            await _userRepository.SetUserRoleAsync(userId, role);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change user {UserId} role to {Role}", userId, role);
            throw new ExternalServiceException("Database", "Failed to change user role", ex);
        }
    }

    private string HashPassword(string password)
    {
        _logger.LogInformation("HashPassword started");
        try
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            _logger.LogInformation("HashPassword succeeded");
            return Convert.ToBase64String(hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash password");
            throw new ExternalServiceException("Cryptography", "Failed to hash password", ex);
        }
    }
}
