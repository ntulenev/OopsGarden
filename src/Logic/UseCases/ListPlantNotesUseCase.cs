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
    /// <param name="plants">The plant repository.</param>
    /// <param name="plantNoteQueries">The plant note query port.</param>
    public ListPlantNotesUseCase(IPlantRepository plants, IPlantNoteQueries plantNoteQueries)
    {
        ArgumentNullException.ThrowIfNull(plants);
        ArgumentNullException.ThrowIfNull(plantNoteQueries);
        _plants = plants;
        _plantNoteQueries = plantNoteQueries;
    }

    /// <inheritdoc />
    public async Task<PlantNotesPage?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var plant = await _plants.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        return await PlantNotesPaging
            .ListAsync(_plantNoteQueries, userId, plantId, page, pageSize, cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly IPlantRepository _plants;
    private readonly IPlantNoteQueries _plantNoteQueries;
}
