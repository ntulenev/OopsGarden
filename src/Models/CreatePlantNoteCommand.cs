namespace Models;

/// <summary>
/// Represents a request to create a plant note.
/// </summary>
/// <param name="Text">The note text.</param>
public sealed record CreatePlantNoteCommand(string Text);
