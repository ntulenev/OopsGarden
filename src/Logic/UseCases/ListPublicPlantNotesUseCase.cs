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
    /// <param name="publicGardenQueries">The public garden query port.</param>
    /// <param name="plantNoteQueries">The plant note query port.</param>
    public ListPublicPlantNotesUseCase(IPublicGardenQueries publicGardenQueries, IPlantNoteQueries plantNoteQueries)
    {
        ArgumentNullException.ThrowIfNull(publicGardenQueries);
        ArgumentNullException.ThrowIfNull(plantNoteQueries);
        _publicGardenQueries = publicGardenQueries;
        _plantNoteQueries = plantNoteQueries;
    }

    /// <inheritdoc />
    public async Task<PlantNotesPage?> ExecuteAsync(
        UserId gardenId,
        PlantId plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var plantExists = await _publicGardenQueries
            .PublicPlantExistsAsync(gardenId, plantId, cancellationToken)
            .ConfigureAwait(false);
        if (!plantExists)
        {
            return null;
        }

        return await PlantNotesPaging
            .ListAsync(_plantNoteQueries, gardenId, plantId, page, pageSize, cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly IPublicGardenQueries _publicGardenQueries;
    private readonly IPlantNoteQueries _plantNoteQueries;
}
