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
    /// Adds a plant photo history item.
    /// </summary>
    Task AddPlantPhotoAsync(
        PlantId plantId,
        ImageDataUrl photoDataUrl,
        DateTimeOffset uploadedAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes a plant note when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> RemovePlantNoteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates a plant note date when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> UpdatePlantNoteCreatedAtAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes a watering event when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> RemoveWateringEventAsync(
        UserId userId,
        PlantId plantId,
        WateringEventId wateringEventId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes a plant.
    /// </summary>
    void RemovePlant(Plant plant);

}
