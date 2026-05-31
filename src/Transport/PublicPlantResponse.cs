namespace Transport;


/// <summary>
/// Represents a public plant response.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="Soil">The plant soil notes.</param>
/// <param name="PhotoDataUrl">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
/// <param name="Location">The optional plant location.</param>
public sealed record PublicPlantResponse(
    Guid Id,
    string Name,
    string Description,
    string Soil,
    string? PhotoDataUrl,
    DateOnly? PlantedOn,
    DateTimeOffset? LastWateredAt,
    PlantLocationResponse? Location)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicPlantResponse"/> record without soil notes.
    /// </summary>
    public PublicPlantResponse(
        Guid id,
        string name,
        string description,
        string? photoDataUrl,
        DateOnly? plantedOn,
        DateTimeOffset? lastWateredAt,
        PlantLocationResponse? location)
        : this(id, name, description, string.Empty, photoDataUrl, plantedOn, lastWateredAt, location)
    {
    }
}
