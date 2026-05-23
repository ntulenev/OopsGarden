using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines user blocking behavior.
/// </summary>
public interface IBlockUserUseCase
{
    /// <summary>
    /// Updates blocked state for a user.
    /// </summary>
    Task<bool> ExecuteAsync(UserId id, bool isBlocked, CancellationToken cancellationToken);
}
