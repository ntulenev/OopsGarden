namespace Models.Application;

/// <summary>
/// Represents a garden plant projection.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="Soil">The plant soil notes.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="Location">The optional plant location.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
/// <param name="HasOverdueReminders">A value indicating whether the plant has active overdue reminders.</param>
public sealed record GardenPlantProjection(
    PlantId Id,
    string Name,
    string Description,
    string Soil,
    string? PhotoData,
    DateOnly? PlantedOn,
    GardenPlantLocationProjection? Location,
    DateTimeOffset? LastWateredAt,
    bool HasOverdueReminders = false)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GardenPlantProjection"/> record without soil notes.
    /// </summary>
    public GardenPlantProjection(
        PlantId id,
        string name,
        string description,
        string? photoData,
        DateOnly? plantedOn,
        GardenPlantLocationProjection? location,
        DateTimeOffset? lastWateredAt)
        : this(id, name, description, string.Empty, photoData, plantedOn, location, lastWateredAt, false)
    {
    }
}
