using Models;

namespace Abstractions;

/// <summary>
/// Represents a public plant application model.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
/// <param name="Location">The optional plant location.</param>
public sealed record PublicGardenPlant(
    PlantId Id,
    string Name,
    string Description,
    string? PhotoData,
    DateOnly? PlantedOn,
    DateTimeOffset? LastWateredAt,
    GardenPlantLocation? Location);
