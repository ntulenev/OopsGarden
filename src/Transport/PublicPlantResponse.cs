namespace Transport;


/// <summary>
/// Represents a public plant response.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="PhotoDataUrl">The optional plant photo data URL.</param>
/// <param name="Location">The optional plant location.</param>
public sealed record PublicPlantResponse(
    Guid Id,
    string Name,
    string Description,
    string? PhotoDataUrl,
    PlantLocationResponse? Location);
