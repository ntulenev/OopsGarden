using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IRenameLocationUseCase" />
public sealed class RenameLocationUseCase : IRenameLocationUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameLocationUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public RenameLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<LocationSummary?> ExecuteAsync(
        UserId userId,
        LocationId id,
        LocationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var location = await _unitOfWork.Garden
            .FindLocationAsync(userId, id, cancellationToken)
            .ConfigureAwait(false);
        if (location is null)
        {
            return null;
        }

        location.Rename(LocationName.From(command.Name));
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new LocationSummary(location.Id, location.Name.Value, location.Plants.Count);
    }

    private readonly IUnitOfWork _unitOfWork;
}
