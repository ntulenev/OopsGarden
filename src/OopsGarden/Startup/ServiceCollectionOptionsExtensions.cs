namespace OopsGarden.Startup;

/// <summary>
/// Provides options registration extensions.
/// </summary>
internal static class ServiceCollectionOptionsExtensions
{
    /// <summary>
    /// Registers OopsGarden configuration options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection.</returns>
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
