using Abstractions.Repositories;
using Abstractions.System;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListGardenPlantsUseCase" />
public sealed class ListGardenPlantsUseCase : IListGardenPlantsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListGardenPlantsUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="clock">The application clock.</param>
    public ListGardenPlantsUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var plants = await _unitOfWork.GardenQueries.ListPlantsAsync(userId, today, cancellationToken).ConfigureAwait(false);
        return [.. plants
            .Select(plant => new PlantSummary(
                plant.Id,
                plant.Name,
                plant.Description,
                plant.Soil,
                plant.PhotoData,
                plant.PlantedOn,
                GardenUseCaseMapping.ToGardenPlantLocation(plant.Location),
                plant.LastWateredAt,
                plant.HasOverdueReminders))];
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
