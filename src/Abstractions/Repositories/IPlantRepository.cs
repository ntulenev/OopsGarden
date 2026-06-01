using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for plant aggregates.
/// </summary>
public interface IPlantRepository
{
    /// <summary>
    /// Finds a plant owned by the user.
    /// </summary>
    Task<Plant?> FindPlantAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a plant.
    /// </summary>
    Task AddPlantAsync(Plant plant, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a plant.
    /// </summary>
    void RemovePlant(Plant plant);

}
