using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines read-only public garden queries.
/// </summary>
public interface IPublicGardenQueries
{
    /// <summary>
    /// Gets a public garden by owner id.
    /// </summary>
    Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a plant belongs to a public garden owner.
    /// </summary>
    Task<bool> PublicPlantExistsAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken);
}
