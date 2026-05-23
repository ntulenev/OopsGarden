namespace Models;

/// <summary>
/// Represents a plant in a user's garden.
/// </summary>
public sealed class Plant
{
    private Plant()
    {
    }

    private Plant(
        PlantId id,
        UserId userId,
        PlantName name,
        PlantDescription description,
        LocationId? locationId,
        DateOnly? plantedOn,
        ImageDataUrl? photoDataUrl,
        DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        CreatedAt = createdAt;
        SetDetails(name, description, locationId, plantedOn, photoDataUrl);
    }

    /// <summary>
    /// Gets the unique plant identifier.
    /// </summary>
    public PlantId Id { get; private set; }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the owning user.
    /// </summary>
    public AppUser? User { get; private set; }

    /// <summary>
    /// Gets the current location identifier.
    /// </summary>
    public LocationId? LocationId { get; private set; }

    /// <summary>
    /// Gets the current location.
    /// </summary>
    public Location? Location { get; private set; }

    /// <summary>
    /// Gets the plant name.
    /// </summary>
    public PlantName Name { get; private set; }

    /// <summary>
    /// Gets the plant description.
    /// </summary>
    public PlantDescription Description { get; private set; }

    /// <summary>
    /// Gets the plant photo as a browser data URL.
    /// </summary>
    public ImageDataUrl? PhotoDataUrl { get; private set; }

    /// <summary>
    /// Gets the planting date.
    /// </summary>
    public DateOnly? PlantedOn { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the watering history.
    /// </summary>
    public IReadOnlyCollection<WateringEvent> WateringEvents => _wateringEvents;

    /// <summary>
    /// Gets the plant life notes.
    /// </summary>
    public IReadOnlyCollection<PlantNote> Notes => _notes;

    /// <summary>
    /// Creates a new plant.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="name">The plant name.</param>
    /// <param name="description">The plant description.</param>
    /// <param name="locationId">The current location identifier.</param>
    /// <param name="plantedOn">The planting date.</param>
    /// <param name="photoDataUrl">The plant photo as a browser data URL.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>A new <see cref="Plant"/> instance.</returns>
    public static Plant Create(
        UserId userId,
        PlantName name,
        PlantDescription description,
        LocationId? locationId,
        DateOnly? plantedOn,
        string? photoDataUrl,
        DateTimeOffset createdAt = default)
        => new(
            PlantId.New(),
            userId,
            name,
            description,
            locationId,
            plantedOn,
            ImageDataUrl.PlantPhoto(photoDataUrl),
            createdAt);

    /// <summary>
    /// Rehydrates a plant from persisted values.
    /// </summary>
    /// <param name="id">The persisted plant identifier.</param>
    /// <param name="userId">The persisted owning user identifier.</param>
    /// <param name="name">The persisted plant name.</param>
    /// <param name="description">The persisted plant description.</param>
    /// <param name="locationId">The persisted current location identifier.</param>
    /// <param name="plantedOn">The persisted planting date.</param>
    /// <param name="photoDataUrl">The persisted plant photo as a browser data URL.</param>
    /// <param name="createdAt">The persisted creation timestamp.</param>
    /// <returns>A rehydrated <see cref="Plant"/> instance.</returns>
    public static Plant Restore(
        PlantId id,
        UserId userId,
        PlantName name,
        PlantDescription description,
        LocationId? locationId,
        DateOnly? plantedOn,
        ImageDataUrl? photoDataUrl,
        DateTimeOffset createdAt)
        => new(
            id,
            userId,
            name,
            description,
            locationId,
            plantedOn,
            photoDataUrl,
            createdAt);

    /// <summary>
    /// Updates editable plant details.
    /// </summary>
    /// <param name="name">The plant name.</param>
    /// <param name="description">The plant description.</param>
    /// <param name="locationId">The current location identifier.</param>
    /// <param name="plantedOn">The planting date.</param>
    /// <param name="photoDataUrl">The plant photo as a browser data URL.</param>
    public void UpdateDetails(
        PlantName name,
        PlantDescription description,
        LocationId? locationId,
        DateOnly? plantedOn,
        string? photoDataUrl)
    {
        SetDetails(
            name,
            description,
            locationId,
            plantedOn,
            ImageDataUrl.PlantPhoto(photoDataUrl));
    }

    private void SetDetails(
        PlantName name,
        PlantDescription description,
        LocationId? locationId,
        DateOnly? plantedOn,
        ImageDataUrl? photoDataUrl)
    {
        Name = name;
        Description = description;
        LocationId = locationId;
        PlantedOn = plantedOn;
        PhotoDataUrl = photoDataUrl;
    }

    /// <summary>
    /// Records that the plant has been watered.
    /// </summary>
    /// <param name="wateredAt">The watering timestamp.</param>
    /// <returns>The created watering event.</returns>
    public WateringEvent Water(DateTimeOffset wateredAt = default)
    {
        var watering = WateringEvent.Create(Id, wateredAt);
        _wateringEvents.Add(watering);
        return watering;
    }

    /// <summary>
    /// Adds a note to the plant life journal.
    /// </summary>
    /// <param name="text">The note text.</param>
    /// <param name="isAutomatic">A value indicating whether the note was created by the system.</param>
    /// <param name="createdAt">The note creation timestamp.</param>
    /// <returns>The created note.</returns>
    public PlantNote AddNote(PlantNoteText text, bool isAutomatic = false, DateTimeOffset createdAt = default)
    {
        var note = PlantNote.Create(Id, text, isAutomatic, createdAt);
        _notes.Add(note);
        return note;
    }

    private readonly List<WateringEvent> _wateringEvents = [];
    private readonly List<PlantNote> _notes = [];
}
