namespace Transport;

/// <summary>
/// Represents a page of plant notes.
/// </summary>
/// <param name="Items">The notes in this page.</param>
/// <param name="Page">The current one-based page.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="Total">The total note count.</param>
/// <param name="HasPrevious">A value indicating whether there is a previous page.</param>
/// <param name="HasNext">A value indicating whether there is a next page.</param>
public sealed record PlantNotesPageResponse(
    IReadOnlyList<PlantNoteResponse> Items,
    int Page,
    int PageSize,
    int Total,
    bool HasPrevious,
    bool HasNext);
