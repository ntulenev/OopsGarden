namespace Models.Application;

/// <summary>
/// Represents a plant shown in a public garden.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="Soil">The plant soil notes.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
/// <param name="Location">The optional plant location.</param>
public sealed record PublicGardenPlantProjection(
    PlantId Id,
    string Name,
    string Description,
    string Soil,
    string? PhotoData,
    DateOnly? PlantedOn,
    DateTimeOffset? LastWateredAt,
    GardenPlantLocationProjection? Location)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicGardenPlantProjection"/> record without soil notes.
    /// </summary>
    public PublicGardenPlantProjection(
        PlantId id,
        string name,
        string description,
        string? photoData,
        DateOnly? plantedOn,
        DateTimeOffset? lastWateredAt,
        GardenPlantLocationProjection? location)
        : this(id, name, description, string.Empty, photoData, plantedOn, lastWateredAt, location)
    {
    }
}
