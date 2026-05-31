namespace Models.Application;

/// <summary>
/// Represents editable plant input.
/// </summary>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="Soil">The plant soil notes.</param>
/// <param name="LocationId">The optional garden location id.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="LastWateredOn">The optional last watering date.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
public sealed record PlantCommand(
    string Name,
    string Description,
    string Soil,
    Guid? LocationId,
    DateOnly? PlantedOn,
    DateOnly? LastWateredOn,
    string? PhotoData)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlantCommand"/> record without soil notes.
    /// </summary>
    public PlantCommand(
        string name,
        string description,
        Guid? locationId,
        DateOnly? plantedOn,
        DateOnly? lastWateredOn,
        string? photoData)
        : this(name, description, string.Empty, locationId, plantedOn, lastWateredOn, photoData)
    {
    }
}
