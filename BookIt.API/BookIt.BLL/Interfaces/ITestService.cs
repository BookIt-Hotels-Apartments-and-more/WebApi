namespace BookIt.BLL.Interfaces;

public interface ITestService
{
    Task<bool> CanConnectToDatabase();
}
