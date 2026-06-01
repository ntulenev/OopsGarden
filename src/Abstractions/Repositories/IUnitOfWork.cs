namespace Abstractions.Repositories;

/// <summary>
/// Coordinates garden persistence operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Gets user storage operations.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets invite storage operations.
    /// </summary>
    IInviteRepository Invites { get; }

    /// <summary>
    /// Gets plant storage operations.
    /// </summary>
    IPlantRepository Plants { get; }

    /// <summary>
    /// Gets location storage operations.
    /// </summary>
    ILocationRepository Locations { get; }

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
