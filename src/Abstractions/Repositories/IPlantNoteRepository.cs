using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for plant notes.
/// </summary>
public interface IPlantNoteRepository
{
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
    /// Updates a plant note date when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> UpdatePlantNoteCreatedAtAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates a plant note reminder resolved flag when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> UpdatePlantNoteReminderStatusAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        bool isResolved,
        CancellationToken cancellationToken);
}
