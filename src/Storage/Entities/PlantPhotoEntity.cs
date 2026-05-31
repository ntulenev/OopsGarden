namespace Storage.Entities;

/// <summary>
/// Represents a persisted plant photo revision.
/// </summary>
public sealed class PlantPhotoEntity
{
    /// <summary>
    /// Gets or sets the plant photo id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the photographed plant id.
    /// </summary>
    public Guid PlantId { get; set; }

    /// <summary>
    /// Gets or sets the photo data URL.
    /// </summary>
    public string PhotoData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the upload timestamp.
    /// </summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the photographed plant.
    /// </summary>
    public PlantEntity? Plant { get; set; }
}
