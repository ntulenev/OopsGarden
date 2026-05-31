using Abstractions.Repositories;
using Abstractions.System;
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
    /// <param name="clock">The application clock.</param>
    public UpdatePlantUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
        _clock = clock;
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

        var previousPhotoData = plant.PhotoDataUrl?.Value;
        plant.UpdateDetails(
            PlantName.From(command.Name),
            PlantDescription.From(command.Description),
            PlantSoil.From(command.Soil),
            locationResult.LocationId,
            command.PlantedOn,
            command.PhotoData);
        if (command.LastWateredOn.HasValue)
        {
            var wateredAt = new DateTimeOffset(command.LastWateredOn.Value.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
            await _unitOfWork.Plants
                .AddWateringEventAsync(plant.Water(wateredAt), cancellationToken)
                .ConfigureAwait(false);
        }

        if (plant.PhotoDataUrl is { } photoDataUrl && photoDataUrl.Value != previousPhotoData)
        {
            await _unitOfWork.Plants
                .AddPlantPhotoAsync(plant.Id, photoDataUrl, _clock.UtcNow, cancellationToken)
                .ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new UpdatePlantResult(UpdatePlantStatus.Updated, null);
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
