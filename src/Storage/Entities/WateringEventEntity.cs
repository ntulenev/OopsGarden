namespace Storage.Entities;

/// <summary>
/// Represents a persisted watering event.
/// </summary>
public sealed class WateringEventEntity
{
    /// <summary>
    /// Gets or sets the watering event id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the watered plant id.
    /// </summary>
    public Guid PlantId { get; set; }

    /// <summary>
    /// Gets or sets the watering timestamp.
    /// </summary>
    public DateTimeOffset WateredAt { get; set; }

    /// <summary>
    /// Gets or sets the watered plant.
    /// </summary>
    public PlantEntity? Plant { get; set; }
}
