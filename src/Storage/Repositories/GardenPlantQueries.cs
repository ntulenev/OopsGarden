using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides garden plant and location list read queries.
/// </summary>
internal sealed class GardenPlantQueries
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GardenPlantQueries"/> class.
    /// </summary>
    public GardenPlantQueries(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken) =>
        ListPlantsAsync(userId, DateOnly.MinValue, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(
        UserId userId,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var plants = await _dbContext.Plants
            .AsNoTracking()
            .Where(plant => plant.UserId == userId.Value)
            .OrderBy(plant => plant.Name)
            .Select(plant => new
            {
                plant.Id,
                plant.Name,
                plant.Description,
                plant.Soil,
                plant.PhotoData,
                plant.PlantedOn,
                Location = plant.Location == null
                    ? null
                    : new { plant.Location.Id, plant.Location.Name },
                LastWateredAt = plant.WateringEvents
                    .OrderByDescending(watering => watering.WateredAt)
                    .Select(watering => (DateTimeOffset?)watering.WateredAt)
                    .FirstOrDefault(),
                HasOverdueReminders = plant.Notes.Any(note =>
                    note.IsReminder &&
                    !note.IsReminderResolved &&
                    note.ReminderDate.HasValue &&
                    note.ReminderDate.Value < today)
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. plants
            .Select(plant => new GardenPlantProjection(
                PlantId.From(plant.Id),
                plant.Name,
                plant.Description,
                plant.Soil,
                plant.PhotoData,
                plant.PlantedOn,
                plant.Location is null
                    ? null
                    : new GardenPlantLocationProjection(LocationId.From(plant.Location.Id), plant.Location.Name),
                plant.LastWateredAt,
                plant.HasOverdueReminders))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken)
    {
        var locations = await _dbContext.Locations
            .AsNoTracking()
            .Where(location => location.UserId == userId.Value)
            .Select(location => new
            {
                location.Id,
                location.Name,
                Plants = location.Plants.Count
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. locations
            .OrderBy(location => location.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(location => new GardenLocationProjection(LocationId.From(location.Id), location.Name, location.Plants))];
    }

    private readonly GardenDbContext _dbContext;
}
