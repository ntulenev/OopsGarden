using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides plant note read queries.
/// </summary>
public sealed partial class GardenQueries
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantNoteProjection>> ListPlantNotesAsync(
        UserId userId,
        PlantId plantId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var notes = await _dbContext.PlantNotes
            .AsNoTracking()
            .Where(note => note.PlantId == plantId.Value && note.Plant!.UserId == userId.Value)
            .OrderByDescending(note => note.CreatedAt)
            .ThenByDescending(note => note.Id)
            .Skip(skip)
            .Take(take)
            .Select(note => new
            {
                note.Id,
                note.Text,
                note.CreatedAt,
                note.IsAutomatic,
                note.IsReminder,
                note.ReminderDate,
                note.IsReminderResolved
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. notes.Select(note => new PlantNoteProjection(
            PlantNoteId.From(note.Id),
            note.Text,
            note.CreatedAt,
            note.IsAutomatic,
            note.IsReminder,
            note.ReminderDate,
            note.IsReminderResolved))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantNoteProjection>> ListOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var notes = await _dbContext.PlantNotes
            .AsNoTracking()
            .Where(note =>
                note.PlantId == plantId.Value &&
                note.Plant!.UserId == userId.Value &&
                note.IsReminder &&
                !note.IsReminderResolved &&
                note.ReminderDate.HasValue &&
                note.ReminderDate.Value < today)
            .OrderBy(note => note.ReminderDate)
            .ThenByDescending(note => note.CreatedAt)
            .ThenByDescending(note => note.Id)
            .Skip(skip)
            .Take(take)
            .Select(note => new
            {
                note.Id,
                note.Text,
                note.CreatedAt,
                note.IsAutomatic,
                note.IsReminder,
                note.ReminderDate,
                note.IsReminderResolved
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. notes.Select(note => new PlantNoteProjection(
            PlantNoteId.From(note.Id),
            note.Text,
            note.CreatedAt,
            note.IsAutomatic,
            note.IsReminder,
            note.ReminderDate,
            note.IsReminderResolved))];
    }

    /// <inheritdoc />
    public Task<int> CountPlantNotesAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        _dbContext.PlantNotes
            .AsNoTracking()
            .CountAsync(note => note.PlantId == plantId.Value && note.Plant!.UserId == userId.Value, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        CancellationToken cancellationToken) =>
        _dbContext.PlantNotes
            .AsNoTracking()
            .CountAsync(
                note =>
                    note.PlantId == plantId.Value &&
                    note.Plant!.UserId == userId.Value &&
                    note.IsReminder &&
                    !note.IsReminderResolved &&
                    note.ReminderDate.HasValue &&
                    note.ReminderDate.Value < today,
                cancellationToken);
}
