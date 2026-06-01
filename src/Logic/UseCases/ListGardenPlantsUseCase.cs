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
    /// <param name="gardenPlantQueries">The garden plant query port.</param>
    /// <param name="clock">The application clock.</param>
    public ListGardenPlantsUseCase(IGardenPlantQueries gardenPlantQueries, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(gardenPlantQueries);
        ArgumentNullException.ThrowIfNull(clock);
        _gardenPlantQueries = gardenPlantQueries;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var plants = await _gardenPlantQueries.ListPlantsAsync(userId, today, cancellationToken).ConfigureAwait(false);
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

    private readonly IGardenPlantQueries _gardenPlantQueries;
    private readonly IClock _clock;
}
