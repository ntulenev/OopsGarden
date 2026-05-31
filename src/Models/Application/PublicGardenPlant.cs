namespace Models.Application;

/// <summary>
/// Represents a public plant application model.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="Soil">The plant soil notes.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
/// <param name="Location">The optional plant location.</param>
public sealed record PublicGardenPlant(
    PlantId Id,
    string Name,
    string Description,
    string Soil,
    string? PhotoData,
    DateOnly? PlantedOn,
    DateTimeOffset? LastWateredAt,
    GardenPlantLocation? Location)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicGardenPlant"/> record without soil notes.
    /// </summary>
    public PublicGardenPlant(
        PlantId id,
        string name,
        string description,
        string? photoData,
        DateOnly? plantedOn,
        DateTimeOffset? lastWateredAt,
        GardenPlantLocation? location)
        : this(id, name, description, string.Empty, photoData, plantedOn, lastWateredAt, location)
    {
    }
}
