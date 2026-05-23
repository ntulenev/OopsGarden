using Abstractions.Services;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Entities;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core plant watering history operations.
/// </summary>
public sealed class PlantWateringHistory : IPlantWateringHistory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlantWateringHistory"/> class.
    /// </summary>
    public PlantWateringHistory(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task ReplaceAsync(PlantId plantId, DateOnly? lastWateredOn, CancellationToken cancellationToken)
    {
        var wateringEvents = await _dbContext.WateringEvents
            .Where(watering => watering.PlantId == plantId.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        _dbContext.WateringEvents.RemoveRange(wateringEvents);

        if (!lastWateredOn.HasValue)
        {
            return;
        }

        var wateredAt = new DateTimeOffset(lastWateredOn.Value.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
        _ = _dbContext.WateringEvents.Add(new WateringEventEntity
        {
            Id = WateringEventId.New().Value,
            PlantId = plantId.Value,
            WateredAt = wateredAt
        });
    }

    private readonly GardenDbContext _dbContext;
}
