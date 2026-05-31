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
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="clock">The application clock.</param>
    public ListOverduePlantRemindersUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
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
        var plant = await _unitOfWork.Plants.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        return await PlantNotesPaging
            .ListOverdueRemindersAsync(_unitOfWork.GardenQueries, userId, plantId, today, page, pageSize, cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
