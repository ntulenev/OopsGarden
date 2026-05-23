namespace Models;

/// <summary>
/// Represents a note in a plant life journal.
/// </summary>
public sealed class PlantNote
{
    private PlantNote()
    {
    }

    private PlantNote(PlantNoteId id, PlantId plantId, PlantNoteText text, DateTimeOffset createdAt)
    {
        Id = id;
        PlantId = plantId;
        Text = text;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the unique note identifier.
    /// </summary>
    public PlantNoteId Id { get; private set; }

    /// <summary>
    /// Gets the noted plant identifier.
    /// </summary>
    public PlantId PlantId { get; private set; }

    /// <summary>
    /// Gets the noted plant.
    /// </summary>
    public Plant? Plant { get; private set; }

    /// <summary>
    /// Gets the note text.
    /// </summary>
    public PlantNoteText Text { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new plant note.
    /// </summary>
    /// <param name="plantId">The noted plant identifier.</param>
    /// <param name="text">The note text.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>A new <see cref="PlantNote"/> instance.</returns>
    public static PlantNote Create(PlantId plantId, PlantNoteText text, DateTimeOffset createdAt = default) =>
        new(PlantNoteId.New(), plantId, text, createdAt);

    /// <summary>
    /// Rehydrates a plant note from persisted values.
    /// </summary>
    /// <param name="id">The persisted note identifier.</param>
    /// <param name="plantId">The persisted plant identifier.</param>
    /// <param name="text">The persisted note text.</param>
    /// <param name="createdAt">The persisted creation timestamp.</param>
    /// <returns>A rehydrated <see cref="PlantNote"/> instance.</returns>
    public static PlantNote Restore(PlantNoteId id, PlantId plantId, PlantNoteText text, DateTimeOffset createdAt) =>
        new(id, plantId, text, createdAt);
}
