namespace Transport;

/// <summary>
/// Represents a request to create or update a garden location.
/// </summary>
/// <param name="Name">The location name.</param>
public sealed record LocationRequest(string Name);
