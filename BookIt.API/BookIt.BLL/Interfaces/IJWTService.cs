using BookIt.BLL.DTOs;
using System.Security.Claims;

namespace BookIt.BLL.Interfaces;

public interface IJWTService
{
    Task<string> GenerateToken(UserAuthDTO user);
    ClaimsPrincipal? ValidateToken(string token);
    bool IsTokenExpired(string token);
    string? GetUserIdFromToken(string token);
}
