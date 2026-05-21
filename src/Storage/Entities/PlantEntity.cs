namespace Storage.Entities;

/// <summary>
/// Represents a persisted plant.
/// </summary>
public sealed class PlantEntity
{
    /// <summary>
    /// Gets or sets the plant id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the location id.
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Gets or sets the plant name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plant description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plant photo data URL.
    /// </summary>
    public string? PhotoData { get; set; }

    /// <summary>
    /// Gets or sets the planting date.
    /// </summary>
    public DateOnly? PlantedOn { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the owning user.
    /// </summary>
    public AppUserEntity? User { get; set; }

    /// <summary>
    /// Gets or sets the current location.
    /// </summary>
    public LocationEntity? Location { get; set; }

    /// <summary>
    /// Gets the watering history.
    /// </summary>
    public ICollection<WateringEventEntity> WateringEvents { get; } = [];
}
