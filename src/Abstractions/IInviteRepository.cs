using Models;

namespace Abstractions;

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

/// <summary>
/// Represents invite data needed by the administration surface.
/// </summary>
/// <param name="Id">The invite id.</param>
/// <param name="Code">The invite code.</param>
/// <param name="CreatedAt">The invite creation timestamp.</param>
/// <param name="CreatedBy">The administrator who created the invite.</param>
/// <param name="UsedAt">The optional invite usage timestamp.</param>
/// <param name="UsedByUserId">The optional id of the user who consumed the invite.</param>
/// <param name="IsRevoked">A value indicating whether the invite is revoked.</param>
public sealed record AdminInviteProjection(
    InviteId Id,
    string Code,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? UsedAt,
    UserId? UsedByUserId,
    bool IsRevoked);
