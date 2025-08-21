using BookIt.BLL.Interfaces;
using BookIt.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookIt.BLL.Extensions;

public static class ServicesRegistrationExtension
{
    public static IServiceCollection AddBLLServices(this IServiceCollection services)
    {
        services.AddScoped<IApartmentsService, ApartmentsService>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IBookingsService, BookingsService>();
        services.AddScoped<IClassificationService, ClassificationService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<IEstablishmentsService, EstablishmentsService>();
        services.AddScoped<IFavoritesService, FavoritesService>();
        services.AddScoped<IGeolocationService, GeolocationService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IImagesService, ImagesService>();
        services.AddScoped<IJWTService, JWTService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IRatingsService, RatingsService>();
        services.AddScoped<IReviewsService, ReviewsService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IUserService, UserService>();

        services.AddHttpClient<IMonobankAcquiringService, MonobankAcquiringService>();

        return services;
    }
}
