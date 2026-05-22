using Abstractions.Repositories;
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
    public WaterPlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<DateTimeOffset?> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, PlantId.From(id), cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var watering = plant.Water();
        await _unitOfWork.Garden.AddWateringEventAsync(watering, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return watering.WateredAt;
    }

    private readonly IUnitOfWork _unitOfWork;
}
