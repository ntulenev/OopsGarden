using System.Text.Json.Serialization;

namespace Transport;

/// <summary>
/// Represents a request to create or update a plant.
/// </summary>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="Soil">The plant soil notes.</param>
/// <param name="LocationId">The current location identifier.</param>
/// <param name="PlantedOn">The planting date.</param>
/// <param name="LastWateredOn">The last watering date.</param>
/// <param name="PhotoDataUrl">The plant photo as a browser data URL.</param>
[method: JsonConstructor]
public sealed record PlantRequest(
    string Name,
    string Description,
    string Soil,
    Guid? LocationId,
    DateOnly? PlantedOn,
    DateOnly? LastWateredOn,
    string? PhotoDataUrl)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlantRequest"/> record without soil notes.
    /// </summary>
    public PlantRequest(
        string name,
        string description,
        Guid? locationId,
        DateOnly? plantedOn,
        DateOnly? lastWateredOn,
        string? photoDataUrl)
        : this(name, description, string.Empty, locationId, plantedOn, lastWateredOn, photoDataUrl)
    {
    }
}
