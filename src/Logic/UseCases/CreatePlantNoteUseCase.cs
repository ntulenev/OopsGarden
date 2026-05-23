using Abstractions.Repositories;
using Abstractions.System;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="ICreatePlantNoteUseCase" />
public sealed class CreatePlantNoteUseCase : ICreatePlantNoteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlantNoteUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="clock">The application clock.</param>
    public CreatePlantNoteUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<PlantNoteSummary?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        CreatePlantNoteCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var plant = await _unitOfWork.Plants.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var note = plant.AddNote(PlantNoteText.From(command.Text), command.IsAutomatic, _clock.UtcNow);
        await _unitOfWork.Plants.AddPlantNoteAsync(note, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new PlantNoteSummary(note.Id, note.Text.Value, note.CreatedAt, note.IsAutomatic);
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
