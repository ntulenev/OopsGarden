using Abstractions.Repositories;

using Models;

namespace Logic.UseCases;

/// <summary>
/// Provides shared plant note paging behavior.
/// </summary>
internal static class PlantNotesPaging
{
    private const int DEFAULT_PAGE_SIZE = 5;
    private const int MAX_PAGE_SIZE = 20;

    /// <summary>
    /// Lists a normalized page of plant notes.
    /// </summary>
    /// <param name="gardenQueries">The garden query port.</param>
    /// <param name="userId">The owning user id.</param>
    /// <param name="plantId">The plant id.</param>
    /// <param name="page">The requested one-based page.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A normalized page of plant note summaries.</returns>
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
            [.. notes.Select(note => new PlantNoteSummary(
                note.Id,
                note.Text,
                note.CreatedAt,
                note.IsAutomatic,
                note.IsReminder,
                note.ReminderDate,
                note.IsReminderResolved))],
            normalizedPage,
            normalizedPageSize,
            total);
    }

    /// <summary>
    /// Lists a normalized page of overdue reminder notes.
    /// </summary>
    /// <param name="gardenQueries">The garden query port.</param>
    /// <param name="userId">The owning user id.</param>
    /// <param name="plantId">The plant id.</param>
    /// <param name="today">The current date.</param>
    /// <param name="page">The requested one-based page.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A normalized page of overdue reminder summaries.</returns>
    public static async Task<PlantNotesPage> ListOverdueRemindersAsync(
        IGardenQueries gardenQueries,
        UserId userId,
        PlantId plantId,
        DateOnly today,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gardenQueries);
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = pageSize <= 0 ? DEFAULT_PAGE_SIZE : Math.Min(pageSize, MAX_PAGE_SIZE);
        var total = await gardenQueries
            .CountOverduePlantRemindersAsync(userId, plantId, today, cancellationToken)
            .ConfigureAwait(false);
        var notes = await gardenQueries
            .ListOverduePlantRemindersAsync(
                userId,
                plantId,
                today,
                (normalizedPage - 1) * normalizedPageSize,
                normalizedPageSize,
                cancellationToken)
            .ConfigureAwait(false);

        return new PlantNotesPage(
            [.. notes.Select(note => new PlantNoteSummary(
                note.Id,
                note.Text,
                note.CreatedAt,
                note.IsAutomatic,
                note.IsReminder,
                note.ReminderDate,
                note.IsReminderResolved))],
            normalizedPage,
            normalizedPageSize,
            total);
    }
}
