using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines read-only plant history queries.
/// </summary>
public interface IPlantHistoryQueries
{
    /// <summary>
    /// Lists all watering events, notes, and photos for the specified plant.
    /// </summary>
    Task<IReadOnlyList<PlantHistoryItemProjection>> ListPlantHistoryAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken);
}
