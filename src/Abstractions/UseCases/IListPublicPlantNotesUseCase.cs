using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines public plant notes listing behavior.
/// </summary>
public interface IListPublicPlantNotesUseCase
{
    /// <summary>
    /// Lists notes for a plant in a public garden.
    /// </summary>
    /// <param name="gardenId">The public garden owner id.</param>
    /// <param name="plantId">The plant id.</param>
    /// <param name="page">The requested one-based page.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A page of notes, or null when the garden or plant is not publicly accessible.</returns>
    Task<PlantNotesPage?> ExecuteAsync(
        UserId gardenId,
        PlantId plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
