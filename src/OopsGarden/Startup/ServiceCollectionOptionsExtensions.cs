using OopsGarden.Configuration;

namespace OopsGarden.Startup;

internal static class ServiceCollectionOptionsExtensions
{
    public static IServiceCollection AddOopsGardenOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        _ = services
            .AddOptions<AdminOptions>()
            .Bind(configuration.GetSection("Admins"))
            .Validate(options => options.Users.Count > 0, "At least one admin must be configured.")
            .ValidateOnStart();

        return services;
    }
}
