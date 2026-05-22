using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListPlantNotesUseCase" />
public sealed class ListPlantNotesUseCase : IListPlantNotesUseCase
{
    private const int DEFAULT_PAGE_SIZE = 5;
    private const int MAX_PAGE_SIZE = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListPlantNotesUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListPlantNotesUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PlantNotesPage?> ExecuteAsync(
        UserId userId,
        Guid plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var id = PlantId.From(plantId);
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = pageSize <= 0 ? DEFAULT_PAGE_SIZE : Math.Min(pageSize, MAX_PAGE_SIZE);
        var total = await _unitOfWork.Garden.CountPlantNotesAsync(userId, id, cancellationToken).ConfigureAwait(false);
        var notes = await _unitOfWork.Garden
            .ListPlantNotesAsync(userId, id, (normalizedPage - 1) * normalizedPageSize, normalizedPageSize, cancellationToken)
            .ConfigureAwait(false);

        return new PlantNotesPage(
            [.. notes.Select(note => new PlantNoteSummary(note.Id, note.Text, note.CreatedAt))],
            normalizedPage,
            normalizedPageSize,
            total);
    }

    private readonly IUnitOfWork _unitOfWork;
}
