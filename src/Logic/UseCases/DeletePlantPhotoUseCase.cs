using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IDeletePlantPhotoUseCase" />
public sealed class DeletePlantPhotoUseCase : IDeletePlantPhotoUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlantPhotoUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeletePlantPhotoUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Plants.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return CommandResult.NotFound;
        }

        var photo = await _unitOfWork.PlantPhotos
            .FindPlantPhotoAsync(userId, plantId, photoId, cancellationToken)
            .ConfigureAwait(false);
        if (photo is null)
        {
            return CommandResult.NotFound;
        }

        var latestPhoto = await _unitOfWork.PlantPhotos
            .FindLatestPlantPhotoAsync(userId, plantId, null, cancellationToken)
            .ConfigureAwait(false);
        if (latestPhoto?.Id == photo.Id || plant.PhotoDataUrl?.Value == photo.PhotoDataUrl)
        {
            var previousPhoto = await _unitOfWork.PlantPhotos
                .FindLatestPlantPhotoAsync(userId, plantId, photoId, cancellationToken)
                .ConfigureAwait(false);
            plant.UpdateDetails(
                plant.Name,
                plant.Description,
                plant.Soil,
                plant.LocationId,
                plant.PlantedOn,
                previousPhoto?.PhotoDataUrl);
        }

        var deleted = await _unitOfWork.PlantPhotos
            .RemovePlantPhotoAsync(userId, plantId, photoId, cancellationToken)
            .ConfigureAwait(false);
        if (!deleted)
        {
            return CommandResult.NotFound;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return CommandResult.Succeeded;
    }

    private readonly IUnitOfWork _unitOfWork;
}
