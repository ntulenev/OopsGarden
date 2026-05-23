using Abstractions.Repositories;
using Abstractions.System;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IWaterPlantUseCase" />
public sealed class WaterPlantUseCase : IWaterPlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaterPlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="clock">The application clock.</param>
    public WaterPlantUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<DateTimeOffset?> ExecuteAsync(UserId userId, PlantId id, CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var watering = plant.Water(_clock.UtcNow);
        await _unitOfWork.Garden.AddWateringEventAsync(watering, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return watering.WateredAt;
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
