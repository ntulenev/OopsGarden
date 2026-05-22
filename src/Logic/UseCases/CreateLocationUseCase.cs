using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="ICreateLocationUseCase" />
public sealed class CreateLocationUseCase : ICreateLocationUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateLocationUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public CreateLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<LocationSummary> ExecuteAsync(
        UserId userId,
        LocationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var location = Location.Create(userId, LocationName.From(command.Name));
        await _unitOfWork.Garden.AddLocationAsync(location, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new LocationSummary(location.Id, location.Name.Value, 0);
    }

    private readonly IUnitOfWork _unitOfWork;
}
