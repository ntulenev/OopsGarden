using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines read-only garden queries.
/// </summary>
public interface IGardenQueries
{
    /// <summary>
    /// Gets a public garden by owner id.
    /// </summary>
    Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a plant belongs to a public garden owner.
    /// </summary>
    Task<bool> PublicPlantExistsAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists plants for a garden owner.
    /// </summary>
    Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists locations for a garden owner.
    /// </summary>
    Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken);

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
    /// Lists all watering events and notes for the specified plant.
    /// </summary>
    Task<IReadOnlyList<PlantHistoryItemProjection>> ListPlantHistoryAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts notes for the specified plant.
    /// </summary>
    Task<int> CountPlantNotesAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken);
}
