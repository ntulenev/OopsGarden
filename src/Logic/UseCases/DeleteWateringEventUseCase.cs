using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IDeleteWateringEventUseCase" />
public sealed class DeleteWateringEventUseCase : IDeleteWateringEventUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteWateringEventUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeleteWateringEventUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        WateringEventId wateringEventId,
        CancellationToken cancellationToken)
    {
        var deleted = await _unitOfWork.Plants
            .RemoveWateringEventAsync(userId, plantId, wateringEventId, cancellationToken)
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
