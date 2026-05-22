namespace Abstractions;

/// <summary>
/// Defines invite revoke behavior.
/// </summary>
public interface IRevokeInviteUseCase
{
    /// <summary>
    /// Revokes an invite.
    /// </summary>
    Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}
