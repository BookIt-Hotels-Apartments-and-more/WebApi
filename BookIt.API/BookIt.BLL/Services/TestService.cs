using BookIt.BLL.Interfaces;
using BookIt.DAL.Database;

namespace BookIt.BLL.Services;

public class TestService : ITestService
{
    private readonly BookingDbContext _dbContext;

    public TestService(BookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CanConnectToDatabase()
    {
        return await _dbContext.Database.CanConnectAsync();
    }
}
