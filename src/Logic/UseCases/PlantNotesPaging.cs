using Abstractions.Repositories;

using Models;

namespace Logic.UseCases;

internal static class PlantNotesPaging
{
    private const int DEFAULT_PAGE_SIZE = 5;
    private const int MAX_PAGE_SIZE = 20;

    public static async Task<PlantNotesPage> ListAsync(
        IGardenQueries gardenQueries,
        UserId userId,
        PlantId plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gardenQueries);
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = pageSize <= 0 ? DEFAULT_PAGE_SIZE : Math.Min(pageSize, MAX_PAGE_SIZE);
        var total = await gardenQueries.CountPlantNotesAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        var notes = await gardenQueries
            .ListPlantNotesAsync(userId, plantId, (normalizedPage - 1) * normalizedPageSize, normalizedPageSize, cancellationToken)
            .ConfigureAwait(false);

        return new PlantNotesPage(
            [.. notes.Select(note => new PlantNoteSummary(note.Id, note.Text, note.CreatedAt))],
            normalizedPage,
            normalizedPageSize,
            total);
    }
}
