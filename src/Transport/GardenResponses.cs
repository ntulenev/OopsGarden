namespace Transport;

/// <summary>
/// Represents a public garden response.
/// </summary>
public sealed record PublicGardenResponse(Guid Id, string Name, string? Avatar, IReadOnlyList<PublicPlantResponse> Plants);

/// <summary>
/// Represents a public plant response.
/// </summary>
public sealed record PublicPlantResponse(
    Guid Id,
    string Name,
    string Description,
    string? PhotoDataUrl,
    PlantLocationResponse? Location);

/// <summary>
/// Represents a garden plant response.
/// </summary>
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
public sealed record PlantLocationResponse(Guid Id, string Name);

/// <summary>
/// Represents a garden location response.
/// </summary>
public sealed record LocationSummaryResponse(Guid Id, string Name, int Plants);
