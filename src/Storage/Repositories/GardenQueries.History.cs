using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides plant history read queries.
/// </summary>
public sealed partial class GardenQueries
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantHistoryItemProjection>> ListPlantHistoryAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken)
    {
        var notes = await _dbContext.PlantNotes
            .AsNoTracking()
            .Where(note => note.PlantId == plantId.Value && note.Plant!.UserId == userId.Value)
            .Select(note => new PlantHistoryItemProjection(
                note.Id,
                "note",
                note.CreatedAt,
                note.Text,
                note.IsAutomatic,
                note.IsReminder,
                note.ReminderDate,
                note.IsReminderResolved))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var waterings = await _dbContext.WateringEvents
            .AsNoTracking()
            .Where(watering => watering.PlantId == plantId.Value && watering.Plant!.UserId == userId.Value)
            .Select(watering => new PlantHistoryItemProjection(watering.Id, "watering", watering.WateredAt, null, false))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var photos = await _dbContext.PlantPhotos
            .AsNoTracking()
            .Where(photo => photo.PlantId == plantId.Value && photo.Plant!.UserId == userId.Value)
            .Select(photo => new PlantHistoryItemProjection(
                photo.Id,
                "photo",
                photo.UploadedAt,
                null,
                false,
                false,
                null,
                false,
                photo.PhotoData))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. notes
            .Concat(waterings)
            .Concat(photos)
            .OrderByDescending(item => item.OccurredAt)
            .ThenByDescending(item => item.Id)];
    }
}
