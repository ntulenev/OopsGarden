

namespace Logic.UseCases;

/// <inheritdoc cref="IUpdatePlantUseCase" />
public sealed class UpdatePlantUseCase : IUpdatePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public UpdatePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<UpdatePlantResult> ExecuteAsync(
        UserId userId,
        Guid id,
        PlantCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var plantId = PlantId.From(id);
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
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
        await _unitOfWork.Garden.ReplaceWateringHistoryAsync(plantId, command.LastWateredOn, cancellationToken)
            .ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new UpdatePlantResult(UpdatePlantStatus.Updated, null);
    }

    private readonly IUnitOfWork _unitOfWork;
}
