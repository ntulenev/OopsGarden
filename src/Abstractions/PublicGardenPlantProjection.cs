using Models;

namespace Abstractions;

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
