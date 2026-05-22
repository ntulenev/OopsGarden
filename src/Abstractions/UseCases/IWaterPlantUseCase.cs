namespace Abstractions.UseCases;

/// <summary>
/// Defines plant watering behavior.
/// </summary>
public interface IWaterPlantUseCase
{
    /// <summary>
    /// Adds a watering event for a plant.
    /// </summary>
    Task<DateTimeOffset?> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken);
}
