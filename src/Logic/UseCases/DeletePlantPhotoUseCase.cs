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
    public async Task<bool> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var deleted = await _unitOfWork.Plants
            .RemovePlantPhotoAsync(userId, plantId, photoId, cancellationToken)
            .ConfigureAwait(false);
        if (!deleted)
        {
            return false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
