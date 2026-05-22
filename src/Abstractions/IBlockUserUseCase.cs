namespace Abstractions;

/// <summary>
/// Defines user blocking behavior.
/// </summary>
public interface IBlockUserUseCase
{
    /// <summary>
    /// Updates blocked state for a user.
    /// </summary>
    Task<bool> ExecuteAsync(Guid id, bool isBlocked, CancellationToken cancellationToken);
}
