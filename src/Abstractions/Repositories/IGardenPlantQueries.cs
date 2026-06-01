using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines read-only garden plant and location queries.
/// </summary>
public interface IGardenPlantQueries
{
    /// <summary>
    /// Lists plants for a garden owner.
    /// </summary>
    Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists plants for a garden owner with reminder state relative to the specified date.
    /// </summary>
    Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(
        UserId userId,
        DateOnly today,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists locations for a garden owner.
    /// </summary>
    Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken);
}
