using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListPlantNotesUseCase" />
public sealed class ListPlantNotesUseCase : IListPlantNotesUseCase
{
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
        PlantId plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        return await PlantNotesPaging
            .ListAsync(_unitOfWork.GardenQueries, userId, plantId, page, pageSize, cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly IUnitOfWork _unitOfWork;
}
