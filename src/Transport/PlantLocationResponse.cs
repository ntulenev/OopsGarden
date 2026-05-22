namespace Transport;


/// <summary>
/// Represents a plant location response.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
public sealed record PlantLocationResponse(Guid Id, string Name);
