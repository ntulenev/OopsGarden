using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IListGardenLocationsUseCase" />
public sealed class ListGardenLocationsUseCase : IListGardenLocationsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListGardenLocationsUseCase"/> class.
    /// </summary>
    /// <param name="gardenPlantQueries">The garden plant query port.</param>
    public ListGardenLocationsUseCase(IGardenPlantQueries gardenPlantQueries)
    {
        ArgumentNullException.ThrowIfNull(gardenPlantQueries);
        _gardenPlantQueries = gardenPlantQueries;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocationSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var locations = await _gardenPlantQueries.ListLocationsAsync(userId, cancellationToken).ConfigureAwait(false);
        return [.. locations
            .Select(location => new LocationSummary(location.Id, location.Name, location.Plants))
        ];
    }

    private readonly IGardenPlantQueries _gardenPlantQueries;
}
