using Models;

namespace Abstractions;

/// <summary>
/// Represents a persisted plant note projection.
/// </summary>
/// <param name="Id">The note id.</param>
/// <param name="Text">The note text.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
public sealed record PlantNoteProjection(PlantNoteId Id, string Text, DateTimeOffset CreatedAt);
