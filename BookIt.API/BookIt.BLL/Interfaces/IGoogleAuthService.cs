namespace BookIt.BLL.Services;

public interface IGoogleAuthService
{
    string GetLoginUrl();
    Task<(string Email, string Name)> GetUserEmailAndNameAsync(string code);
}
