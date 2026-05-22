
using Models;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="IDeletePlantUseCase" />
internal sealed class DeletePlantUseCase : IDeletePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeletePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, PlantId.From(id), cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return false;
        }

        _unitOfWork.Garden.RemovePlant(plant);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
