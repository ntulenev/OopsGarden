using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines read-only plant note queries.
/// </summary>
public interface IPlantNoteQueries
{
    /// <summary>
    /// Lists notes for the specified plant.
    /// </summary>
    Task<IReadOnlyList<PlantNoteProjection>> ListPlantNotesAsync(
        UserId userId,
        PlantId plantId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists overdue unresolved reminders for the specified plant.
    /// </summary>
    Task<IReadOnlyList<PlantNoteProjection>> ListOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts notes for the specified plant.
    /// </summary>
    Task<int> CountPlantNotesAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken);

    /// <summary>
    /// Counts overdue unresolved reminders for the specified plant.
    /// </summary>
    Task<int> CountOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        CancellationToken cancellationToken);
}
