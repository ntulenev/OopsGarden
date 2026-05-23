using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines invite deletion behavior.
/// </summary>
public interface IDeleteInviteUseCase
{
    /// <summary>
    /// Deletes an invite.
    /// </summary>
    Task<DeleteInviteResult> ExecuteAsync(InviteId id, CancellationToken cancellationToken);
}
