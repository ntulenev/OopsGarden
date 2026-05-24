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
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListPublicPlantHistoryUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantHistoryItem>?> ExecuteAsync(
        UserId gardenId,
        PlantId plantId,
        CancellationToken cancellationToken)
    {
        var garden = await _unitOfWork.GardenQueries.GetPublicGardenAsync(gardenId, cancellationToken).ConfigureAwait(false);
        if (garden is null || !garden.Plants.Any(plant => plant.Id == plantId))
        {
            return null;
        }

        var items = await _unitOfWork.GardenQueries
            .ListPlantHistoryAsync(gardenId, plantId, cancellationToken)
            .ConfigureAwait(false);

        return [.. items.Select(GardenUseCaseMapping.ToPlantHistoryItem)];
    }

    private readonly IUnitOfWork _unitOfWork;
}
