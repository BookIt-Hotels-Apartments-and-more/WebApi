using BookIt.BLL.DTOs;
using System.Security.Claims;

namespace BookIt.BLL.Interfaces;

public interface IJWTService
{
    string GenerateToken(UserDTO user);
    ClaimsPrincipal? ValidateToken(string token);
    bool IsTokenExpired(string token);
    string? GetUserIdFromToken(string token);
}
