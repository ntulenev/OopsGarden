using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IUpdatePlantNoteDateUseCase" />
public sealed class UpdatePlantNoteDateUseCase : IUpdatePlantNoteDateUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlantNoteDateUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public UpdatePlantNoteDateUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        UpdatePlantNoteDateCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var createdAt = new DateTimeOffset(command.CreatedOn.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
        var updated = await _unitOfWork.Plants
            .UpdatePlantNoteCreatedAtAsync(userId, plantId, noteId, createdAt, cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
        {
            return false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
