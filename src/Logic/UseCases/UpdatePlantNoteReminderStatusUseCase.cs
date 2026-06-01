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
    public async Task<CommandResult> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        bool isResolved,
        CancellationToken cancellationToken)
    {
        var updated = await _unitOfWork.PlantNotes
            .UpdatePlantNoteReminderStatusAsync(userId, plantId, noteId, isResolved, cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
        {
            return CommandResult.NotFound;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return CommandResult.Succeeded;
    }

    private readonly IUnitOfWork _unitOfWork;
}
