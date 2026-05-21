namespace Transport;

/// <summary>
/// Represents a request to create or update a plant.
/// </summary>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="LocationId">The current location identifier.</param>
/// <param name="PlantedOn">The planting date.</param>
/// <param name="PhotoDataUrl">The plant photo as a browser data URL.</param>
public sealed record PlantRequest(
    string Name,
    string Description,
    Guid? LocationId,
    DateOnly? PlantedOn,
    string? PhotoDataUrl);
