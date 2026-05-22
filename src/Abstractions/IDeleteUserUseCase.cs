namespace Abstractions;

/// <summary>
/// Defines user deletion behavior.
/// </summary>
public interface IDeleteUserUseCase
{
    /// <summary>
    /// Deletes a user.
    /// </summary>
    Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}
