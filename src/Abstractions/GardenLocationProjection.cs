using Models;

namespace Abstractions;

/// <summary>
/// Represents a garden location projection.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
/// <param name="Plants">The number of plants assigned to the location.</param>
public sealed record GardenLocationProjection(LocationId Id, string Name, int Plants);
