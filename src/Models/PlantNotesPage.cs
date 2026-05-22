namespace Models;

/// <summary>
/// Represents a page of plant notes.
/// </summary>
/// <param name="Items">The note items.</param>
/// <param name="Page">The current one-based page.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="Total">The total note count.</param>
public sealed record PlantNotesPage(IReadOnlyList<PlantNoteSummary> Items, int Page, int PageSize, int Total)
{
    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNext => Page * PageSize < Total;
}
