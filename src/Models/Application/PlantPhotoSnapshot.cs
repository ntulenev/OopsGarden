namespace Models.Application;

/// <summary>
/// Represents a persisted plant photo snapshot.
/// </summary>
/// <param name="Id">The plant photo id.</param>
/// <param name="PlantId">The photographed plant id.</param>
/// <param name="PhotoDataUrl">The photo data URL.</param>
/// <param name="UploadedAt">The upload timestamp.</param>
public sealed record PlantPhotoSnapshot(Guid Id, PlantId PlantId, string PhotoDataUrl, DateTimeOffset UploadedAt);
