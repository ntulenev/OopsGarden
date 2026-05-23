using Models;

namespace Abstractions.Services;

/// <summary>
/// Provides plant watering history persistence operations.
/// </summary>
public interface IPlantWateringHistory
{
    /// <summary>
    /// Replaces the watering history for a plant with a single date or no date.
    /// </summary>
    Task ReplaceAsync(PlantId plantId, DateOnly? lastWateredOn, CancellationToken cancellationToken);
}
