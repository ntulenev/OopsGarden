namespace Models.Application;

/// <summary>
/// Represents a persisted plant history item projection.
/// </summary>
/// <param name="Id">The history item id.</param>
/// <param name="Type">The history item type.</param>
/// <param name="OccurredAt">The event timestamp used for sorting.</param>
/// <param name="Text">The optional note text.</param>
/// <param name="IsAutomatic">A value indicating whether the note was created by the system.</param>
/// <param name="PhotoDataUrl">The optional plant photo data URL.</param>
public sealed record PlantHistoryItemProjection(
    Guid Id,
    string Type,
    DateTimeOffset OccurredAt,
    string? Text,
    bool IsAutomatic,
    string? PhotoDataUrl = null);
