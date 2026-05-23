namespace Models;

/// <summary>
/// Represents a single watering event for a plant.
/// </summary>
public sealed class WateringEvent
{
    private WateringEvent()
    {
    }

    private WateringEvent(WateringEventId id, PlantId plantId, DateTimeOffset wateredAt)
    {
        Id = id;
        PlantId = plantId;
        WateredAt = wateredAt;
    }

    /// <summary>
    /// Gets the unique watering event identifier.
    /// </summary>
    public WateringEventId Id { get; private set; }

    /// <summary>
    /// Gets the watered plant identifier.
    /// </summary>
    public PlantId PlantId { get; private set; }

    /// <summary>
    /// Gets the watered plant.
    /// </summary>
    public Plant? Plant { get; private set; }

    /// <summary>
    /// Gets the watering timestamp.
    /// </summary>
    public DateTimeOffset WateredAt { get; private set; }

    /// <summary>
    /// Creates a new watering event.
    /// </summary>
    /// <param name="plantId">The watered plant identifier.</param>
    /// <param name="wateredAt">The watering timestamp.</param>
    /// <returns>A new <see cref="WateringEvent"/> instance.</returns>
    public static WateringEvent Create(PlantId plantId, DateTimeOffset wateredAt = default)
        => new(WateringEventId.New(), plantId, wateredAt);

    /// <summary>
    /// Rehydrates a watering event from persisted values.
    /// </summary>
    /// <param name="id">The persisted watering event identifier.</param>
    /// <param name="plantId">The persisted watered plant identifier.</param>
    /// <param name="wateredAt">The persisted watering timestamp.</param>
    /// <returns>A rehydrated <see cref="WateringEvent"/> instance.</returns>
    public static WateringEvent Restore(WateringEventId id, PlantId plantId, DateTimeOffset wateredAt)
        => new(id, plantId, wateredAt);
}
