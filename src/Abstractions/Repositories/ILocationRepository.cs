using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for location aggregates.
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Finds a location owned by the user.
    /// </summary>
    Task<Location?> FindLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a location exists for the user.
    /// </summary>
    Task<bool> LocationExistsAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a location.
    /// </summary>
    Task AddLocationAsync(Location location, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a location.
    /// </summary>
    void RemoveLocation(Location location);

    /// <summary>
    /// Clears the location assignment from plants in the specified location.
    /// </summary>
    Task ClearPlantLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken);
}
