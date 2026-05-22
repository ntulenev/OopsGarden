using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines the delete plant note use case.
/// </summary>
public interface IDeletePlantNoteUseCase
{
    /// <summary>
    /// Deletes a plant note.
    /// </summary>
    /// <param name="userId">The owning user id.</param>
    /// <param name="plantId">The plant id.</param>
    /// <param name="noteId">The note id.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True when the note was deleted.</returns>
    Task<bool> ExecuteAsync(UserId userId, Guid plantId, Guid noteId, CancellationToken cancellationToken);
}
