namespace BookIt.BLL.Interfaces;

public interface IGoogleAuthService
{
    string GetLoginUrl();
    Task<(string Email, string Name, string? ImageUrl)> GetUserInfoAsync(string code);
}
