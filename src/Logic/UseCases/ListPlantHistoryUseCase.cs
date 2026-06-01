using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListPlantHistoryUseCase" />
public sealed class ListPlantHistoryUseCase : IListPlantHistoryUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListPlantHistoryUseCase"/> class.
    /// </summary>
    /// <param name="plants">The plant repository.</param>
    /// <param name="plantHistoryQueries">The plant history query port.</param>
    public ListPlantHistoryUseCase(IPlantRepository plants, IPlantHistoryQueries plantHistoryQueries)
    {
        ArgumentNullException.ThrowIfNull(plants);
        ArgumentNullException.ThrowIfNull(plantHistoryQueries);
        _plants = plants;
        _plantHistoryQueries = plantHistoryQueries;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantHistoryItem>?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken)
    {
        var plant = await _plants.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var items = await _plantHistoryQueries
            .ListPlantHistoryAsync(userId, plantId, cancellationToken)
            .ConfigureAwait(false);

        return [.. items.Select(GardenUseCaseMapping.ToPlantHistoryItem)];
    }

    private readonly IPlantRepository _plants;
    private readonly IPlantHistoryQueries _plantHistoryQueries;
}
