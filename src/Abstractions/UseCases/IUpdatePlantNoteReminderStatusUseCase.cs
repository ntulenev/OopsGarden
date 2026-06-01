using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines plant note reminder status update behavior.
/// </summary>
public interface IUpdatePlantNoteReminderStatusUseCase
{
    /// <summary>
    /// Updates a reminder resolved flag.
    /// </summary>
    Task<CommandResult> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        bool isResolved,
        CancellationToken cancellationToken);
}
