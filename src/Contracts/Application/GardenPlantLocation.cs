using Models;

namespace Contracts.Application;

/// <summary>
/// Represents a plant location application model.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
public sealed record GardenPlantLocation(LocationId Id, string Name);
