namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for garden aggregates.
/// </summary>
public interface IGardenRepository : IPlantRepository, ILocationRepository;
