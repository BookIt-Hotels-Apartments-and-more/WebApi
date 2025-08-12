using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookIt.BLL.Services;

public class JWTService : IJWTService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JWTService> _logger;
    private readonly UserRepository _userRepository;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JWTService(
        ILogger<JWTService> logger,
        UserRepository userRepository,
        IOptions<JwtSettings> jwtSettingsOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtSettings = jwtSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(jwtSettingsOptions));

        ValidateJwtConfiguration();

        _securityKey = CreateSecurityKey();
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public string GenerateToken(UserDTO user)
    {
        try
        {
            ValidateUserData(user);

            _logger.LogInformation("Generating JWT token for user {UserId} with email {Email}", user.Id, user.Email);

            var claims = CreateUserClaims(user);
            var signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = CreateTokenDescriptor(claims, signingCredentials);
            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            _userRepository.UpdateUserLastActivityAtAsync(user.Id);

            _logger.LogInformation("Successfully generated JWT token for user {UserId}", user.Id);

            return tokenString;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Security token error while generating JWT for user {UserId}", user?.Id);
            throw new ExternalServiceException("JWT", "Failed to create security token", ex);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument while generating JWT for user {UserId}", user?.Id);
            throw new ExternalServiceException("JWT", "Invalid token generation parameters", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while generating JWT for user {UserId}", user?.Id);
            throw new ExternalServiceException("JWT", "Failed to generate JWT token", ex);
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ValidationException("Token", "JWT token cannot be null or empty");

            _logger.LogInformation("Validating JWT token");

            var tokenValidationParameters = CreateTokenValidationParameters();

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ExternalServiceException("JWT", "Invalid token algorithm");
            }

            _logger.LogInformation("Successfully validated JWT token");
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "JWT token has expired");
            throw new BusinessRuleViolationException("TOKEN_EXPIRED", "JWT token has expired");
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning(ex, "JWT token has invalid signature");
            throw new BusinessRuleViolationException("INVALID_TOKEN_SIGNATURE", "JWT token signature is invalid");
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            throw new BusinessRuleViolationException("INVALID_TOKEN", "JWT token is invalid");
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while validating JWT token");
            throw new ExternalServiceException("JWT", "Failed to validate JWT token", ex);
        }
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return true;

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch (ArgumentException)
        {
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if token is expired");
            return true;
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            return userIdClaim?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from token");
            return null;
        }
    }

    private void ValidateJwtConfiguration()
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
            validationErrors.Add("Secret", new List<string> { "JWT secret is required" });

        if (_jwtSettings.Secret?.Length < 32)
            validationErrors.Add("Secret", new List<string> { "JWT secret must be at least 32 characters long" });

        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
            validationErrors.Add("Issuer", new List<string> { "JWT issuer is required" });

        if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            validationErrors.Add("Audience", new List<string> { "JWT audience is required" });

        if (_jwtSettings.ExpiryInHours <= 0)
            validationErrors.Add("ExpiryInHours", new List<string> { "JWT expiry must be greater than 0 hours" });

        if (_jwtSettings.ExpiryInHours > 24 * 7)
            validationErrors.Add("ExpiryInHours", new List<string> { "JWT expiry cannot exceed 7 days (168 hours)" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateUserData(UserDTO user)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (user is null) validationErrors.Add("User", new List<string> { "User data is required" });
        if (user?.Id <= 0) validationErrors.Add("UserId", new List<string> { "Valid user ID is required" });
        if (string.IsNullOrWhiteSpace(user?.Email)) validationErrors.Add("Email", new List<string> { "User email is required" });

        if (!string.IsNullOrWhiteSpace(user?.Email) &&
            !System.Text.RegularExpressions.Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            validationErrors.Add("Email", new List<string> { "Invalid email format" });
        }

        if (!Enum.IsDefined(typeof(DAL.Enums.UserRole), user?.Role ?? 0))
            validationErrors.Add("Role", new List<string> { "Valid user role is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private SymmetricSecurityKey CreateSecurityKey()
    {
        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
            return new SymmetricSecurityKey(keyBytes);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("JWT", "Failed to create security key", ex);
        }
    }

    private List<Claim> CreateUserClaims(UserDTO user)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (!string.IsNullOrWhiteSpace(user.Username))
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

            return claims;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("JWT", "Failed to create user claims", ex);
        }
    }

    private SecurityTokenDescriptor CreateTokenDescriptor(List<Claim> claims, SigningCredentials signingCredentials)
    {
        try
        {
            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryInHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = signingCredentials
            };
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("JWT", "Failed to create token descriptor", ex);
        }
    }

    private TokenValidationParameters CreateTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _securityKey,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
    }
}