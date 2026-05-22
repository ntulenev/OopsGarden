
using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListGardenPlantsUseCase" />
public sealed class ListGardenPlantsUseCase : IListGardenPlantsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListGardenPlantsUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListGardenPlantsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var plants = await _unitOfWork.Garden.ListPlantsAsync(userId, cancellationToken).ConfigureAwait(false);
        return [.. plants
            .Select(plant => new PlantSummary(
                plant.Id,
                plant.Name,
                plant.Description,
                plant.PhotoData,
                plant.PlantedOn,
                GardenUseCaseMapping.ToResponse(plant.Location),
                plant.LastWateredAt))];
    }

    private readonly IUnitOfWork _unitOfWork;
}
