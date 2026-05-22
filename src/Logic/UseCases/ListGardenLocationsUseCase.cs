
using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListGardenLocationsUseCase" />
public sealed class ListGardenLocationsUseCase : IListGardenLocationsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListGardenLocationsUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListGardenLocationsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocationSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var locations = await _unitOfWork.Garden.ListLocationsAsync(userId, cancellationToken).ConfigureAwait(false);
        return [.. locations
            .Select(location => new LocationSummary(location.Id, location.Name, location.Plants))
        ];
    }

    private readonly IUnitOfWork _unitOfWork;
}
