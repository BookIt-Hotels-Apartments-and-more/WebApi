using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using BookIt.DAL.Configuration.Settings;
using BookIt.BLL.DTOs;

namespace BookIt.BLL.Services;

public class JWTService : IJWTService
{
    private readonly IOptions<JwtSettings> _jwtSettingsOptions;

    public JWTService(IOptions<JwtSettings> jwtSettingsOptions)
    {
        _jwtSettingsOptions = jwtSettingsOptions;
    }

    public string GenerateToken(UserDTO user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsOptions.Value.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettingsOptions.Value.Issuer,
            audience: _jwtSettingsOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
