using Microsoft.AspNetCore.Identity;

using Models;

namespace OopsGarden.Startup;

internal static class ServiceCollectionApplicationExtensions
{
    public static IServiceCollection AddOopsGardenApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddScoped<PasswordHasher<AppUser>>();

        return services;
    }
}
