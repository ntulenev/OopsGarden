using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines user deletion behavior.
/// </summary>
public interface IDeleteUserUseCase
{
    /// <summary>
    /// Deletes a user.
    /// </summary>
    Task<bool> ExecuteAsync(UserId id, CancellationToken cancellationToken);
}
