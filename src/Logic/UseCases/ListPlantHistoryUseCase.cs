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
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListPlantHistoryUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantHistoryItem>?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Plants.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var items = await _unitOfWork.GardenQueries
            .ListPlantHistoryAsync(userId, plantId, cancellationToken)
            .ConfigureAwait(false);

        return [.. items.Select(item => new PlantHistoryItem(item.Id, item.Type, item.OccurredAt, item.Text, item.IsAutomatic))];
    }

    private readonly IUnitOfWork _unitOfWork;
}
