using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListPublicPlantNotesUseCase" />
public sealed class ListPublicPlantNotesUseCase : IListPublicPlantNotesUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListPublicPlantNotesUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListPublicPlantNotesUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PlantNotesPage?> ExecuteAsync(
        Guid gardenId,
        Guid plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var userId = UserId.From(gardenId);
        var id = PlantId.From(plantId);
        var garden = await _unitOfWork.GardenQueries.GetPublicGardenAsync(userId, cancellationToken).ConfigureAwait(false);
        if (garden is null || !garden.Plants.Any(plant => plant.Id == id))
        {
            return null;
        }

        return await PlantNotesPaging
            .ListAsync(_unitOfWork.GardenQueries, userId, id, page, pageSize, cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly IUnitOfWork _unitOfWork;
}
