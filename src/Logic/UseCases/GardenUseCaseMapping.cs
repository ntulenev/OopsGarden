using Abstractions.Repositories;

using Models;

namespace Logic.UseCases;

/// <summary>
/// Maps garden persistence projections to application models.
/// </summary>
internal static class GardenUseCaseMapping
{
    /// <summary>
    /// Converts a garden plant location projection to an application model.
    /// </summary>
    /// <param name="location">The optional location projection.</param>
    /// <returns>The optional garden plant location model.</returns>
    public static GardenPlantLocation? ToGardenPlantLocation(GardenPlantLocationProjection? location) =>
        location is null ? null : new GardenPlantLocation(location.Id, location.Name);

    /// <summary>
    /// Converts a plant history projection to an application model.
    /// </summary>
    /// <param name="item">The plant history projection.</param>
    /// <returns>The plant history item model.</returns>
    public static PlantHistoryItem ToPlantHistoryItem(PlantHistoryItemProjection item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new PlantHistoryItem(item.Id, item.Type, item.OccurredAt, item.Text, item.IsAutomatic);
    }

    /// <summary>
    /// Resolves and validates a plant location id for a user.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="userId">The owner user id.</param>
    /// <param name="id">The optional requested location id.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The resolved location result.</returns>
    public static async Task<ResolveLocationResult> ResolveLocationIdAsync(
        IUnitOfWork unitOfWork,
        UserId userId,
        Guid? id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        if (!id.HasValue)
        {
            return new ResolveLocationResult(null, null);
        }

        var locationId = LocationId.From(id.Value);
        return await unitOfWork.Locations.LocationExistsAsync(userId, locationId, cancellationToken).ConfigureAwait(false)
            ? new ResolveLocationResult(locationId, null)
            : new ResolveLocationResult(null, PlantCommandError.InvalidLocation);
    }
}
