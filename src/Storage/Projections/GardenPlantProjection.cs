using Models;

namespace Storage.Projections;

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
