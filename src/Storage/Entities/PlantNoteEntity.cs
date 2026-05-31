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
    /// Gets or sets a value indicating whether the note was created by the system.
    /// </summary>
    public bool IsAutomatic { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the note is a reminder.
    /// </summary>
    public bool IsReminder { get; set; }

    /// <summary>
    /// Gets or sets the reminder due date.
    /// </summary>
    public DateOnly? ReminderDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reminder is resolved.
    /// </summary>
    public bool IsReminderResolved { get; set; }

    /// <summary>
    /// Gets or sets the noted plant.
    /// </summary>
    public PlantEntity? Plant { get; set; }
}
