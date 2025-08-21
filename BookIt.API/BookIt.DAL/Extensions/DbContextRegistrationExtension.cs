using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookIt.DAL.Extensions;

public static class DbContextRegistrationExtension
{
    public static IServiceCollection AddCustomDbContext<DbContextType>(this IServiceCollection services, IConfiguration config)
        where DbContextType : DbContext
    {
        services.AddDbContext<DbContextType>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                ?? config.GetRequiredSection("ConnectionStrings:SQLDatabase").Value;
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
