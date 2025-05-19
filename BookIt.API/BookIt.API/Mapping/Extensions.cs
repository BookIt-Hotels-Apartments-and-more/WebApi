using BookIt.API.Mapping.MappingProfiles;

namespace BookIt.API.Mapping;

public static class ServicesExtensions
{
    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(EstablishmentsMappingProfile));
        return services;
    }
}
