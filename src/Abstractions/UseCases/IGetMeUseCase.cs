using System.Security.Claims;

namespace Abstractions.UseCases;

/// <summary>
/// Defines current-session lookup behavior.
/// </summary>
public interface IGetMeUseCase
{
    /// <summary>
    /// Gets current principal information.
    /// </summary>
    Task<CurrentUser> ExecuteAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}
