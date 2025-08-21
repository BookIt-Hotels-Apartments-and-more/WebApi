using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookIt.DAL.Extensions;

public static class DbContextRegistrationExtension
{
    public static IServiceCollection AddCustomDbContext<DbContextType>(this IServiceCollection services)
        where DbContextType : DbContext
    {
        services.AddDbContext<DbContextType>((serviceProvider, options) =>
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                   ?? serviceProvider.GetRequiredService<IConfiguration>()
                                   .GetRequiredSection("ConnectionStrings:SQLDatabase").Value;

            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
