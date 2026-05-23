using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines plant deletion behavior.
/// </summary>
public interface IDeletePlantUseCase
{
    /// <summary>
    /// Deletes a plant.
    /// </summary>
    Task<bool> ExecuteAsync(UserId userId, PlantId id, CancellationToken cancellationToken);
}
