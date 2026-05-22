namespace Abstractions.UseCases;

/// <summary>
/// Defines plant update behavior.
/// </summary>
public interface IUpdatePlantUseCase
{
    /// <summary>
    /// Updates a plant.
    /// </summary>
    Task<UpdatePlantResult> ExecuteAsync(UserId userId, Guid id, PlantCommand command, CancellationToken cancellationToken);
}
