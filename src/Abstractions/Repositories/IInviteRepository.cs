namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for registration invites.
/// </summary>
public interface IInviteRepository
{
    /// <summary>
    /// Finds an invite by code.
    /// </summary>
    Task<InviteLink?> FindByCodeAsync(InviteCode code, CancellationToken cancellationToken);

    /// <summary>
    /// Finds an invite by id.
    /// </summary>
    Task<InviteLink?> FindByIdAsync(InviteId id, CancellationToken cancellationToken);

    /// <summary>
    /// Lists invites for administration.
    /// </summary>
    Task<IReadOnlyList<AdminInviteProjection>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new invite.
    /// </summary>
    Task AddAsync(InviteLink invite, CancellationToken cancellationToken);

    /// <summary>
    /// Removes an invite.
    /// </summary>
    void Remove(InviteLink invite);
}
