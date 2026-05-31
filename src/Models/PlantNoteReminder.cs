namespace Models;

/// <summary>
/// Represents reminder metadata for a plant note.
/// </summary>
/// <param name="IsReminder">A value indicating whether the note is a reminder.</param>
/// <param name="ReminderDate">The reminder due date.</param>
/// <param name="IsResolved">A value indicating whether the reminder is resolved.</param>
public sealed record PlantNoteReminder(bool IsReminder, DateOnly? ReminderDate, bool IsResolved)
{
    /// <summary>
    /// Gets empty reminder metadata.
    /// </summary>
    public static PlantNoteReminder None { get; } = new(false, null, false);

    /// <summary>
    /// Creates reminder metadata.
    /// </summary>
    /// <param name="reminderDate">The reminder due date.</param>
    /// <param name="isResolved">A value indicating whether the reminder is resolved.</param>
    /// <returns>A new <see cref="PlantNoteReminder"/> instance.</returns>
    public static PlantNoteReminder Create(DateOnly reminderDate, bool isResolved = false) =>
        new(true, reminderDate, isResolved);
}
