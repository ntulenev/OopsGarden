using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines watering event deletion behavior.
/// </summary>
public interface IDeleteWateringEventUseCase
{
    /// <summary>
    /// Deletes a watering event when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        WateringEventId wateringEventId,
        CancellationToken cancellationToken);
}
