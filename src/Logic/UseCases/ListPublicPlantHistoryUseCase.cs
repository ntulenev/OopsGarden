using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListPublicPlantHistoryUseCase" />
public sealed class ListPublicPlantHistoryUseCase : IListPublicPlantHistoryUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListPublicPlantHistoryUseCase"/> class.
    /// </summary>
    /// <param name="publicGardenQueries">The public garden query port.</param>
    /// <param name="plantHistoryQueries">The plant history query port.</param>
    public ListPublicPlantHistoryUseCase(IPublicGardenQueries publicGardenQueries, IPlantHistoryQueries plantHistoryQueries)
    {
        ArgumentNullException.ThrowIfNull(publicGardenQueries);
        ArgumentNullException.ThrowIfNull(plantHistoryQueries);
        _publicGardenQueries = publicGardenQueries;
        _plantHistoryQueries = plantHistoryQueries;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantHistoryItem>?> ExecuteAsync(
        UserId gardenId,
        PlantId plantId,
        CancellationToken cancellationToken)
    {
        var plantExists = await _publicGardenQueries
            .PublicPlantExistsAsync(gardenId, plantId, cancellationToken)
            .ConfigureAwait(false);
        if (!plantExists)
        {
            return null;
        }

        var items = await _plantHistoryQueries
            .ListPlantHistoryAsync(gardenId, plantId, cancellationToken)
            .ConfigureAwait(false);

        return [.. items.Select(GardenUseCaseMapping.ToPlantHistoryItem)];
    }

    private readonly IPublicGardenQueries _publicGardenQueries;
    private readonly IPlantHistoryQueries _plantHistoryQueries;
}
