using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IUpdatePlantNoteReminderStatusUseCase" />
public sealed class UpdatePlantNoteReminderStatusUseCase : IUpdatePlantNoteReminderStatusUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlantNoteReminderStatusUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public UpdatePlantNoteReminderStatusUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        bool isResolved,
        CancellationToken cancellationToken)
    {
        var updated = await _unitOfWork.Plants
            .UpdatePlantNoteReminderStatusAsync(userId, plantId, noteId, isResolved, cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
        {
            return false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
