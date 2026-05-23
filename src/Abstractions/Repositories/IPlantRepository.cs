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
    /// Adds a watering event.
    /// </summary>
    Task AddWateringEventAsync(WateringEvent watering, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a plant note.
    /// </summary>
    Task AddPlantNoteAsync(PlantNote note, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a plant note when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> RemovePlantNoteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes a plant.
    /// </summary>
    void RemovePlant(Plant plant);

}
