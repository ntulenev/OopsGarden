using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IDeleteLocationUseCase" />
public sealed class DeleteLocationUseCase : IDeleteLocationUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLocationUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeleteLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(UserId userId, LocationId id, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Garden.FindLocationAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (location is null)
        {
            return false;
        }

        await _unitOfWork.Garden.ClearPlantLocationAsync(userId, id, cancellationToken).ConfigureAwait(false);
        _unitOfWork.Garden.RemoveLocation(location);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
