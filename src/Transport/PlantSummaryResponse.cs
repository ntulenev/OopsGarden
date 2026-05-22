namespace Transport;


/// <summary>
/// Represents a garden plant response.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="PhotoDataUrl">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="Location">The optional plant location.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
public sealed record PlantSummaryResponse(
    Guid Id,
    string Name,
    string Description,
    string? PhotoDataUrl,
    DateOnly? PlantedOn,
    PlantLocationResponse? Location,
    DateTimeOffset? LastWateredAt);
