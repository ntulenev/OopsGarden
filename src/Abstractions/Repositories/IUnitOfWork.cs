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
    /// Gets garden storage operations.
    /// </summary>
    IGardenRepository Garden { get; }

    /// <summary>
    /// Gets read-only garden queries.
    /// </summary>
    IGardenQueries GardenQueries { get; }

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
