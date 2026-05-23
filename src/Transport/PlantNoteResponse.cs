namespace Transport;

/// <summary>
/// Represents a plant note response.
/// </summary>
/// <param name="Id">The note id.</param>
/// <param name="Text">The note text.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
/// <param name="IsAutomatic">A value indicating whether the note was created by the system.</param>
public sealed record PlantNoteResponse(Guid Id, string Text, DateTimeOffset CreatedAt, bool IsAutomatic);
