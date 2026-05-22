using Models;

namespace Abstractions;

/// <summary>
/// Defines plant deletion behavior.
/// </summary>
public interface IDeletePlantUseCase
{
    /// <summary>
    /// Deletes a plant.
    /// </summary>
    Task<bool> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken);
}
