using Abstractions.Repositories;

using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core location persistence operations.
/// </summary>
public sealed class LocationRepository : ILocationRepository, ISyncChanges
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationRepository"/> class.
    /// </summary>
    public LocationRepository(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<Location?> FindLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Locations
            .Include(location => location.Plants)
            .SingleOrDefaultAsync(
                location => location.Id == locationId.Value && location.UserId == userId.Value,
                cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : Track(entity);
    }

    /// <inheritdoc />
    public Task<bool> LocationExistsAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken) =>
        _dbContext.Locations.AnyAsync(
            location => location.Id == locationId.Value && location.UserId == userId.Value,
            cancellationToken);

    /// <inheritdoc />
    public Task AddLocationAsync(Location location, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(location);
        var entity = location.ToEntity();
        _ = _dbContext.Locations.Add(entity);
        _trackedLocations[location.Id.Value] = (location, entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void RemoveLocation(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (!_trackedLocations.TryGetValue(location.Id.Value, out var tracked))
        {
            tracked = (location, location.ToEntity());
            _dbContext.Locations.Attach(tracked.Entity);
        }

        _ = _dbContext.Locations.Remove(tracked.Entity);
    }

    /// <inheritdoc />
    public async Task ClearPlantLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken)
    {
        var plants = await _dbContext.Plants
            .Where(plant => plant.UserId == userId.Value && plant.LocationId == locationId.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var plant in plants)
        {
            plant.LocationId = null;
        }
    }

    /// <inheritdoc />
    public void SyncChanges()
    {
        foreach (var (location, entity) in _trackedLocations.Values)
        {
            location.CopyTo(entity);
        }
    }

    private Location Track(Entities.LocationEntity entity)
    {
        var location = Location.Restore(
            LocationId.From(entity.Id),
            UserId.From(entity.UserId),
            LocationName.From(entity.Name),
            entity.Plants.Select(plantEntity => plantEntity.ToDomain()));
        _trackedLocations[entity.Id] = (location, entity);
        return location;
    }

    private readonly GardenDbContext _dbContext;
    private readonly Dictionary<Guid, (Location Location, Entities.LocationEntity Entity)> _trackedLocations = [];
}
