
namespace Models.Application;

/// <summary>
/// Represents a plant note application model.
/// </summary>
/// <param name="Id">The note id.</param>
/// <param name="Text">The note text.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
/// <param name="IsAutomatic">A value indicating whether the note was created by the system.</param>
/// <param name="IsReminder">A value indicating whether the note is a reminder.</param>
/// <param name="ReminderDate">The reminder due date.</param>
/// <param name="IsReminderResolved">A value indicating whether the reminder is resolved.</param>
public sealed record PlantNoteSummary(
    PlantNoteId Id,
    string Text,
    DateTimeOffset CreatedAt,
    bool IsAutomatic,
    bool IsReminder = false,
    DateOnly? ReminderDate = null,
    bool IsReminderResolved = false);
