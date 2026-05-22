using Models;

namespace Contracts.Projections;

/// <summary>
/// Represents a plant location projection.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
public sealed record GardenPlantLocationProjection(LocationId Id, string Name);
