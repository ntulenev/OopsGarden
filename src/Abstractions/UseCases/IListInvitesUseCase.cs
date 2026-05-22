using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines invite list behavior.
/// </summary>
public interface IListInvitesUseCase
{
    /// <summary>
    /// Lists invites.
    /// </summary>
    Task<IReadOnlyList<AdminInvite>> ExecuteAsync(CancellationToken cancellationToken);
}
