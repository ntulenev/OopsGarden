using Abstractions.Repositories;
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
    public CreatePlantNoteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PlantNoteSummary?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        CreatePlantNoteCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var note = plant.AddNote(PlantNoteText.From(command.Text));
        await _unitOfWork.Garden.AddPlantNoteAsync(note, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new PlantNoteSummary(note.Id, note.Text.Value, note.CreatedAt);
    }

    private readonly IUnitOfWork _unitOfWork;
}
