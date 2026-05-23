using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines public plant history listing behavior.
/// </summary>
public interface IListPublicPlantHistoryUseCase
{
    /// <summary>
    /// Lists all watering events and notes for a plant in a public garden.
    /// </summary>
    /// <param name="gardenId">The public garden owner id.</param>
    /// <param name="plantId">The plant id.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The plant history, or null when the garden or plant is not publicly accessible.</returns>
    Task<IReadOnlyList<PlantHistoryItem>?> ExecuteAsync(
        UserId gardenId,
        PlantId plantId,
        CancellationToken cancellationToken);
}
