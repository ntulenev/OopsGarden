using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines plant note date update behavior.
/// </summary>
public interface IUpdatePlantNoteDateUseCase
{
    /// <summary>
    /// Updates a plant note date when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        UpdatePlantNoteDateCommand command,
        CancellationToken cancellationToken);
}
