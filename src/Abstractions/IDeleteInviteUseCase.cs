namespace Abstractions;

/// <summary>
/// Defines invite deletion behavior.
/// </summary>
public interface IDeleteInviteUseCase
{
    /// <summary>
    /// Deletes an invite.
    /// </summary>
    Task<DeleteInviteResult> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}
