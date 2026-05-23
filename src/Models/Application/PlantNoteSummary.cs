
namespace Models.Application;

/// <summary>
/// Represents a plant note application model.
/// </summary>
/// <param name="Id">The note id.</param>
/// <param name="Text">The note text.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
/// <param name="IsAutomatic">A value indicating whether the note was created by the system.</param>
public sealed record PlantNoteSummary(PlantNoteId Id, string Text, DateTimeOffset CreatedAt, bool IsAutomatic);
