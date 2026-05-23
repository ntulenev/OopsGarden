using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IDeletePlantNoteUseCase" />
public sealed class DeletePlantNoteUseCase : IDeletePlantNoteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlantNoteUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeletePlantNoteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(UserId userId, PlantId plantId, PlantNoteId noteId, CancellationToken cancellationToken)
    {
        var deleted = await _unitOfWork.Garden
            .RemovePlantNoteAsync(userId, plantId, noteId, cancellationToken)
            .ConfigureAwait(false);
        if (!deleted)
        {
            return false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
