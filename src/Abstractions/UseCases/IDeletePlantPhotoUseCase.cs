using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines plant photo deletion behavior.
/// </summary>
public interface IDeletePlantPhotoUseCase
{
    /// <summary>
    /// Deletes a plant photo when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        Guid photoId,
        CancellationToken cancellationToken);
}
