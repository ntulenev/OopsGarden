using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for plant watering events.
/// </summary>
public interface IWateringEventRepository
{
    /// <summary>
    /// Adds a watering event.
    /// </summary>
    Task AddWateringEventAsync(WateringEvent watering, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a watering event when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> RemoveWateringEventAsync(
        UserId userId,
        PlantId plantId,
        WateringEventId wateringEventId,
        CancellationToken cancellationToken);
}
