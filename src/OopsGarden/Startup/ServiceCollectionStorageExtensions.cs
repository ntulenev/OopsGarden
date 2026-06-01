using Abstractions.Repositories;
using Abstractions.Services;

using Microsoft.EntityFrameworkCore;

using Storage;
using Storage.Repositories;

namespace OopsGarden.Startup;

/// <summary>
/// Provides storage service registration extensions.
/// </summary>
internal static class ServiceCollectionStorageExtensions
{
    /// <summary>
    /// Registers OopsGarden storage services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddOopsGardenStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        _ = services.AddDbContext<GardenDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("OopsGarden");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Missing ConnectionStrings__OopsGarden environment variable.");
            }

            _ = options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(GardenDbContext).Assembly.FullName));
        });

        _ = services.AddScoped<IUserRepository, UsersRepository>();
        _ = services.AddScoped<IInviteRepository, InvitesRepository>();
        _ = services.AddScoped<IPlantRepository, PlantRepository>();
        _ = services.AddScoped<ILocationRepository, LocationRepository>();
        _ = services.AddScoped<IPublicGardenQueries, PublicGardenQueries>();
        _ = services.AddScoped<IGardenPlantQueries, GardenPlantQueries>();
        _ = services.AddScoped<IPlantNoteQueries, PlantNoteQueries>();
        _ = services.AddScoped<IPlantHistoryQueries, PlantHistoryQueries>();
        _ = services.AddScoped<IGardenQueries, GardenQueries>();
        _ = services.AddScoped<IPlantWateringHistory, PlantWateringHistory>();
        _ = services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
