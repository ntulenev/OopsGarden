using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines user login behavior.
/// </summary>
public interface ILoginUseCase
{
    /// <summary>
    /// Authenticates a user.
    /// </summary>
    Task<AuthenticatedUser?> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken);
}
