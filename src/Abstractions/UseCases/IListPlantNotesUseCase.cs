using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines the list plant notes use case.
/// </summary>
public interface IListPlantNotesUseCase
{
    /// <summary>
    /// Lists plant notes.
    /// </summary>
    /// <param name="userId">The owning user id.</param>
    /// <param name="plantId">The plant id.</param>
    /// <param name="page">The requested one-based page.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A page of notes, or null when the plant is missing.</returns>
    Task<PlantNotesPage?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
