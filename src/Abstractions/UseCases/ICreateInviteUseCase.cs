using System.Security.Claims;

namespace Abstractions.UseCases;

/// <summary>
/// Defines invite creation behavior.
/// </summary>
public interface ICreateInviteUseCase
{
    /// <summary>
    /// Creates an invite.
    /// </summary>
    Task<CreatedInvite> ExecuteAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}
