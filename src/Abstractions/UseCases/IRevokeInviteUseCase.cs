using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines invite revoke behavior.
/// </summary>
public interface IRevokeInviteUseCase
{
    /// <summary>
    /// Revokes an invite.
    /// </summary>
    Task<CommandResult> ExecuteAsync(InviteId id, CancellationToken cancellationToken);
}
