namespace Storage.Entities;

/// <summary>
/// Represents a persisted plant note.
/// </summary>
public sealed class PlantNoteEntity
{
    /// <summary>
    /// Gets or sets the plant note id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the noted plant id.
    /// </summary>
    public Guid PlantId { get; set; }

    /// <summary>
    /// Gets or sets the note text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the noted plant.
    /// </summary>
    public PlantEntity? Plant { get; set; }
}
