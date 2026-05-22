namespace Abstractions;

/// <summary>
/// Represents editable plant input.
/// </summary>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="LocationId">The optional garden location id.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="LastWateredOn">The optional last watering date.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
public sealed record PlantCommand(
    string Name,
    string Description,
    Guid? LocationId,
    DateOnly? PlantedOn,
    DateOnly? LastWateredOn,
    string? PhotoData);
