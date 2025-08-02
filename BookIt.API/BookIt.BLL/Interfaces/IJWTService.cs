using BookIt.BLL.DTOs;

namespace BookIt.BLL.Services;

public interface IJWTService
{
    string GenerateToken(UserDTO user);
}
