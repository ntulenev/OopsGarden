namespace Transport;


/// <summary>
/// Represents a garden plant response.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="Soil">The plant soil notes.</param>
/// <param name="PhotoDataUrl">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="Location">The optional plant location.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
/// <param name="HasOverdueReminders">A value indicating whether the plant has active overdue reminders.</param>
public sealed record PlantSummaryResponse(
    Guid Id,
    string Name,
    string Description,
    string Soil,
    string? PhotoDataUrl,
    DateOnly? PlantedOn,
    PlantLocationResponse? Location,
    DateTimeOffset? LastWateredAt,
    bool HasOverdueReminders = false)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlantSummaryResponse"/> record without soil notes.
    /// </summary>
    public PlantSummaryResponse(
        Guid id,
        string name,
        string description,
        string? photoDataUrl,
        DateOnly? plantedOn,
        PlantLocationResponse? location,
        DateTimeOffset? lastWateredAt)
        : this(id, name, description, string.Empty, photoDataUrl, plantedOn, location, lastWateredAt, false)
    {
    }
}
