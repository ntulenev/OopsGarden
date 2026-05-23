namespace Models.Application;

/// <summary>
/// Represents an item in a plant history.
/// </summary>
/// <param name="Id">The history item id.</param>
/// <param name="Type">The history item type.</param>
/// <param name="OccurredAt">The event timestamp used for sorting.</param>
/// <param name="Text">The optional note text.</param>
/// <param name="IsAutomatic">A value indicating whether the note was created by the system.</param>
public sealed record PlantHistoryItem(Guid Id, string Type, DateTimeOffset OccurredAt, string? Text, bool IsAutomatic);
