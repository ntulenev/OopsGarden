namespace Abstractions.UseCases;

/// <summary>
/// Defines location deletion behavior.
/// </summary>
public interface IDeleteLocationUseCase
{
    /// <summary>
    /// Deletes a garden location.
    /// </summary>
    Task<bool> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken);
}
