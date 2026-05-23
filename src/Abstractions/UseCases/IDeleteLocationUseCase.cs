using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines location deletion behavior.
/// </summary>
public interface IDeleteLocationUseCase
{
    /// <summary>
    /// Deletes a garden location.
    /// </summary>
    Task<bool> ExecuteAsync(UserId userId, LocationId id, CancellationToken cancellationToken);
}
