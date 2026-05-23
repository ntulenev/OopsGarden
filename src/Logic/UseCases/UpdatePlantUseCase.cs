using Abstractions.Repositories;
using Abstractions.Services;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IUpdatePlantUseCase" />
public sealed class UpdatePlantUseCase : IUpdatePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="wateringHistory">The watering history persistence service.</param>
    public UpdatePlantUseCase(IUnitOfWork unitOfWork, IPlantWateringHistory wateringHistory)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(wateringHistory);
        _unitOfWork = unitOfWork;
        _wateringHistory = wateringHistory;
    }

    /// <inheritdoc />
    public async Task<UpdatePlantResult> ExecuteAsync(
        UserId userId,
        PlantId id,
        PlantCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var plant = await _unitOfWork.Plants.FindPlantAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return new UpdatePlantResult(UpdatePlantStatus.NotFound, null);
        }

        var locationResult = await GardenUseCaseMapping
            .ResolveLocationIdAsync(_unitOfWork, userId, command.LocationId, cancellationToken)
            .ConfigureAwait(false);
        if (!locationResult.IsSuccess)
        {
            return new UpdatePlantResult(UpdatePlantStatus.Invalid, locationResult.Error);
        }

        plant.UpdateDetails(
            PlantName.From(command.Name),
            PlantDescription.From(command.Description),
            locationResult.LocationId,
            command.PlantedOn,
            command.PhotoData);
        await _wateringHistory.ReplaceAsync(id, command.LastWateredOn, cancellationToken)
            .ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new UpdatePlantResult(UpdatePlantStatus.Updated, null);
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlantWateringHistory _wateringHistory;
}
