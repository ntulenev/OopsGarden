using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines plant history listing behavior.
/// </summary>
public interface IListPlantHistoryUseCase
{
    /// <summary>
    /// Lists all watering events and notes for a plant.
    /// </summary>
    Task<IReadOnlyList<PlantHistoryItem>?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken);
}
