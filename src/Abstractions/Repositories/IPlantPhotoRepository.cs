using Models;

namespace Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for plant photo history.
/// </summary>
public interface IPlantPhotoRepository
{
    /// <summary>
    /// Adds a plant photo history item.
    /// </summary>
    Task AddPlantPhotoAsync(
        PlantId plantId,
        ImageDataUrl photoDataUrl,
        DateTimeOffset uploadedAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Finds a plant photo when it belongs to the specified owner and plant.
    /// </summary>
    Task<PlantPhotoSnapshot?> FindPlantPhotoAsync(
        UserId userId,
        PlantId plantId,
        Guid photoId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Finds the latest plant photo when it belongs to the specified owner and plant.
    /// </summary>
    Task<PlantPhotoSnapshot?> FindLatestPlantPhotoAsync(
        UserId userId,
        PlantId plantId,
        Guid? excludedPhotoId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes a plant photo when it belongs to the specified owner and plant.
    /// </summary>
    Task<bool> RemovePlantPhotoAsync(
        UserId userId,
        PlantId plantId,
        Guid photoId,
        CancellationToken cancellationToken);
}
