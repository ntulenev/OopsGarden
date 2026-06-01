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
    Task<CommandResult> ExecuteAsync(UserId userId, PlantId id, CancellationToken cancellationToken);
}
