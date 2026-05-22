namespace Transport;

/// <summary>
/// Represents a public garden response.
/// </summary>
/// <param name="Id">The garden owner id.</param>
/// <param name="Name">The garden owner display name.</param>
/// <param name="Avatar">The optional garden owner avatar data URL.</param>
/// <param name="Plants">The public plants in the garden.</param>
public sealed record PublicGardenResponse(Guid Id, string Name, string? Avatar, IReadOnlyList<PublicPlantResponse> Plants);

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

/// <summary>
/// Represents a plant location response.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
public sealed record PlantLocationResponse(Guid Id, string Name);

/// <summary>
/// Represents a garden location response.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
/// <param name="Plants">The number of plants assigned to the location.</param>
public sealed record LocationSummaryResponse(Guid Id, string Name, int Plants);
