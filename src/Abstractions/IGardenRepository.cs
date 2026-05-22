using Models;

namespace Abstractions;

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

/// <summary>
/// Represents a garden location projection.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
/// <param name="Plants">The number of plants assigned to the location.</param>
public sealed record GardenLocationProjection(LocationId Id, string Name, int Plants);

/// <summary>
/// Represents a garden plant projection.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="Location">The optional plant location.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
public sealed record GardenPlantProjection(
    PlantId Id,
    string Name,
    string Description,
    string? PhotoData,
    DateOnly? PlantedOn,
    GardenPlantLocationProjection? Location,
    DateTimeOffset? LastWateredAt);

/// <summary>
/// Represents a plant location projection.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
public sealed record GardenPlantLocationProjection(LocationId Id, string Name);

/// <summary>
/// Represents a public garden projection.
/// </summary>
/// <param name="Id">The garden owner id.</param>
/// <param name="Name">The garden owner display name.</param>
/// <param name="Avatar">The optional garden owner avatar data URL.</param>
/// <param name="Plants">The public plants in the garden.</param>
public sealed record PublicGardenProjection(
    UserId Id,
    string Name,
    string? Avatar,
    IReadOnlyList<PublicGardenPlantProjection> Plants);

/// <summary>
/// Represents a plant shown in a public garden.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="Location">The optional plant location.</param>
public sealed record PublicGardenPlantProjection(
    PlantId Id,
    string Name,
    string Description,
    string? PhotoData,
    GardenPlantLocationProjection? Location);
