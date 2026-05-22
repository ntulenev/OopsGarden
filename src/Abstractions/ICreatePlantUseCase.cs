using Models;

namespace Abstractions;

/// <summary>
/// Defines plant creation behavior.
/// </summary>
public interface ICreatePlantUseCase
{
    /// <summary>
    /// Creates a plant.
    /// </summary>
    Task<CreatePlantResult> ExecuteAsync(UserId userId, PlantCommand command, CancellationToken cancellationToken);
}
