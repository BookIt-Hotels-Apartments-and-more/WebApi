using BookIt.API.Mapping.MappingProfiles;

namespace BookIt.API.Mapping;

public static class ServicesExtensions
{
    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(EstablishmentsMappingProfile));
        services.AddAutoMapper(typeof(ApartmentsMappingProfile));
        services.AddAutoMapper(typeof(BookingsMappingProfile));
        services.AddAutoMapper(typeof(FavoritesMappingProfile));
        services.AddAutoMapper(typeof(ImagesMappingProfile));
        services.AddAutoMapper(typeof(GeolocationsMappingProfile));
        services.AddAutoMapper(typeof(ReviewsMappingProfile));
        services.AddAutoMapper(typeof(RatingsMappingProfile));
        services.AddAutoMapper(typeof(UserMappingProfile));
        return services;
    }
}
