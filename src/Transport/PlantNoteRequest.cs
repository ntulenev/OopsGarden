namespace Transport;

/// <summary>
/// Represents a plant note request.
/// </summary>
/// <param name="Text">The note text.</param>
/// <param name="IsAutomatic">A value indicating whether the note was created by the system.</param>
/// <param name="IsReminder">A value indicating whether the note is a reminder.</param>
/// <param name="ReminderDate">The reminder due date.</param>
public sealed record PlantNoteRequest(
    string Text,
    bool IsAutomatic = false,
    bool IsReminder = false,
    DateOnly? ReminderDate = null);
