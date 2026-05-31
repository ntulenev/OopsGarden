using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines plant watering behavior.
/// </summary>
public interface IWaterPlantUseCase
{
    /// <summary>
    /// Adds a watering event for a plant.
    /// </summary>
    Task<DateTimeOffset?> ExecuteAsync(UserId userId, PlantId id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a watering event for a plant on the specified date.
    /// </summary>
    Task<DateTimeOffset?> ExecuteAsync(UserId userId, PlantId id, DateOnly wateredOn, CancellationToken cancellationToken);
}
