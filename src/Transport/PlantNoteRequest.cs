namespace Transport;

/// <summary>
/// Represents a plant note request.
/// </summary>
/// <param name="Text">The note text.</param>
public sealed record PlantNoteRequest(string Text);
