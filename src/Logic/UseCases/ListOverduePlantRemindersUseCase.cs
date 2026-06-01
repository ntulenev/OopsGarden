using Abstractions.Repositories;
using Abstractions.System;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListOverduePlantRemindersUseCase" />
public sealed class ListOverduePlantRemindersUseCase : IListOverduePlantRemindersUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListOverduePlantRemindersUseCase"/> class.
    /// </summary>
    /// <param name="plants">The plant repository.</param>
    /// <param name="plantNoteQueries">The plant note query port.</param>
    /// <param name="clock">The application clock.</param>
    public ListOverduePlantRemindersUseCase(IPlantRepository plants, IPlantNoteQueries plantNoteQueries, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(plants);
        ArgumentNullException.ThrowIfNull(plantNoteQueries);
        ArgumentNullException.ThrowIfNull(clock);
        _plants = plants;
        _plantNoteQueries = plantNoteQueries;
        _clock = clock;
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

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        return await PlantNotesPaging
            .ListOverdueRemindersAsync(_plantNoteQueries, userId, plantId, today, page, pageSize, cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly IPlantRepository _plants;
    private readonly IPlantNoteQueries _plantNoteQueries;
    private readonly IClock _clock;
}
