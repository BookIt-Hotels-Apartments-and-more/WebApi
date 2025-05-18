namespace BookIt.BLL.Services;

using BookIt.DAL.Models;

public interface IJWTService
{
    string GenerateToken(User user);
}
