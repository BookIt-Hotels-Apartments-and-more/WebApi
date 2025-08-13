namespace BookIt.BLL.Interfaces;

public interface IGoogleAuthService
{
    string GetLoginUrl();
    Task<(string Email, string Name)> GetUserEmailAndNameAsync(string code);
}
