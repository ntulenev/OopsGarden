namespace Models.Application;

/// <summary>
/// Represents a request to create a plant note.
/// </summary>
/// <param name="Text">The note text.</param>
/// <param name="IsAutomatic">A value indicating whether the note was created by the system.</param>
public sealed record CreatePlantNoteCommand(string Text, bool IsAutomatic = false);
