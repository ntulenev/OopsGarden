using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines the create plant note use case.
/// </summary>
public interface ICreatePlantNoteUseCase
{
    /// <summary>
    /// Creates a note for a plant.
    /// </summary>
    /// <param name="userId">The owning user id.</param>
    /// <param name="plantId">The plant id.</param>
    /// <param name="command">The create note command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created note, or null when the plant is missing.</returns>
    Task<PlantNoteSummary?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        CreatePlantNoteCommand command,
        CancellationToken cancellationToken);
}
