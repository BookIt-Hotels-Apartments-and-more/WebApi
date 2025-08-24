using BookIt.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BookIt.DAL.Extensions;

public static class RepositoriesRegistrationExtension
{
    public static IServiceCollection AddDALRepositories(this IServiceCollection services)
    {
        services.AddScoped<ApartmentRatingRepository>();
        services.AddScoped<ApartmentsRepository>();
        services.AddScoped<BookingsRepository>();
        services.AddScoped<EstablishmentsRepository>();
        services.AddScoped<FavoritesRepository>();
        services.AddScoped<GeolocationRepository>();
        services.AddScoped<ImagesRepository>();
        services.AddScoped<PaymentRepository>();
        services.AddScoped<ReviewsRepository>();
        services.AddScoped<UserRatingRepository>();
        services.AddScoped<UserRepository>();

        return services;
    }
}
