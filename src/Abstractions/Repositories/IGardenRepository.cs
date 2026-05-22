namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for garden aggregates.
/// </summary>
public interface IGardenRepository
{
    /// <summary>
    /// Gets a public garden by owner id.
    /// </summary>
    Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists plants for a garden owner.
    /// </summary>
    Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists locations for a garden owner.
    /// </summary>
    Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a plant owned by the user.
    /// </summary>
    Task<Plant?> FindPlantAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a location owned by the user.
    /// </summary>
    Task<Location?> FindLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a location exists for the user.
    /// </summary>
    Task<bool> LocationExistsAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a plant.
    /// </summary>
    Task AddPlantAsync(Plant plant, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a location.
    /// </summary>
    Task AddLocationAsync(Location location, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a watering event.
    /// </summary>
    Task AddWateringEventAsync(WateringEvent watering, CancellationToken cancellationToken);

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
    /// Counts notes for the specified plant.
    /// </summary>
    Task<int> CountPlantNotesAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken);

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

    /// <summary>
    /// Removes a location.
    /// </summary>
    void RemoveLocation(Location location);

    /// <summary>
    /// Clears the location assignment from plants in the specified location.
    /// </summary>
    Task ClearPlantLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces the watering history for a plant with a single date or no date.
    /// </summary>
    Task ReplaceWateringHistoryAsync(PlantId plantId, DateOnly? lastWateredOn, CancellationToken cancellationToken);
}
