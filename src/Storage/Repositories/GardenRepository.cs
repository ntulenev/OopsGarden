using Abstractions.Repositories;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Entities;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core garden persistence operations.
/// </summary>
public sealed class GardenRepository : IGardenRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GardenRepository"/> class.
    /// </summary>
    public GardenRepository(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken)
    {
        var garden = await _dbContext.Users
            .Where(user => user.Id == userId.Value && !user.IsBlocked && user.IsGardenPublic)
            .Select(user => new
            {
                user.Id,
                user.DisplayName,
                Avatar = user.AvatarData,
                Plants = user.Plants
                    .Select(plant => new
                    {
                        plant.Id,
                        plant.Name,
                        plant.Description,
                        plant.PhotoData,
                        plant.PlantedOn,
                        LastWateredAt = plant.WateringEvents
                            .OrderByDescending(watering => watering.WateredAt)
                            .Select(watering => (DateTimeOffset?)watering.WateredAt)
                            .FirstOrDefault(),
                        Location = plant.Location == null
                            ? null
                            : new { plant.Location.Id, plant.Location.Name }
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return garden is null
            ? null
            : new PublicGardenProjection(
                UserId.From(garden.Id),
                garden.DisplayName,
                garden.Avatar,
                [.. garden.Plants
                    .Select(plant => new PublicGardenPlantProjection(
                        PlantId.From(plant.Id),
                        plant.Name,
                        plant.Description,
                        plant.PhotoData,
                        plant.PlantedOn,
                        plant.LastWateredAt,
                        plant.Location is null
                            ? null
                            : new GardenPlantLocationProjection(
                                LocationId.From(plant.Location.Id),
                                plant.Location.Name)))]);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken)
    {
        var plants = await _dbContext.Plants
            .Where(plant => plant.UserId == userId.Value)
            .Include(plant => plant.Location)
            .OrderBy(plant => plant.Name)
            .Select(plant => new
            {
                plant.Id,
                plant.Name,
                plant.Description,
                plant.PhotoData,
                plant.PlantedOn,
                Location = plant.Location == null
                    ? null
                    : new { plant.Location.Id, plant.Location.Name },
                LastWateredAt = plant.WateringEvents
                    .OrderByDescending(watering => watering.WateredAt)
                    .Select(watering => (DateTimeOffset?)watering.WateredAt)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. plants
            .Select(plant => new GardenPlantProjection(
                PlantId.From(plant.Id),
                plant.Name,
                plant.Description,
                plant.PhotoData,
                plant.PlantedOn,
                plant.Location is null
                    ? null
                    : new GardenPlantLocationProjection(LocationId.From(plant.Location.Id), plant.Location.Name),
                plant.LastWateredAt))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken)
    {
        var locations = await _dbContext.Locations
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

    /// <inheritdoc />
    public async Task<Plant?> FindPlantAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Plants
            .SingleOrDefaultAsync(plant => plant.Id == plantId.Value && plant.UserId == userId.Value, cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : Track(entity);
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
    public Task AddPlantAsync(Plant plant, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plant);
        var entity = plant.ToEntity();
        _ = _dbContext.Plants.Add(entity);
        _trackedPlants[plant.Id.Value] = (plant, entity);
        return Task.CompletedTask;
    }

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
    public Task AddWateringEventAsync(WateringEvent watering, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(watering);
        _ = _dbContext.WateringEvents.Add(watering.ToEntity());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantNoteProjection>> ListPlantNotesAsync(
        UserId userId,
        PlantId plantId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var notes = await _dbContext.PlantNotes
            .Where(note => note.PlantId == plantId.Value && note.Plant!.UserId == userId.Value)
            .OrderByDescending(note => note.CreatedAt)
            .ThenByDescending(note => note.Id)
            .Skip(skip)
            .Take(take)
            .Select(note => new
            {
                note.Id,
                note.Text,
                note.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. notes.Select(note => new PlantNoteProjection(
            PlantNoteId.From(note.Id),
            note.Text,
            note.CreatedAt))];
    }

    /// <inheritdoc />
    public Task<int> CountPlantNotesAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        _dbContext.PlantNotes
            .CountAsync(note => note.PlantId == plantId.Value && note.Plant!.UserId == userId.Value, cancellationToken);

    /// <inheritdoc />
    public Task AddPlantNoteAsync(PlantNote note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(note);
        _ = _dbContext.PlantNotes.Add(note.ToEntity());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> RemovePlantNoteAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        CancellationToken cancellationToken)
    {
        var note = await _dbContext.PlantNotes
            .SingleOrDefaultAsync(
                note => note.Id == noteId.Value && note.PlantId == plantId.Value && note.Plant!.UserId == userId.Value,
                cancellationToken)
            .ConfigureAwait(false);
        if (note is null)
        {
            return false;
        }

        _ = _dbContext.PlantNotes.Remove(note);
        return true;
    }

    /// <inheritdoc />
    public void RemovePlant(Plant plant)
    {
        ArgumentNullException.ThrowIfNull(plant);
        if (!_trackedPlants.TryGetValue(plant.Id.Value, out var tracked))
        {
            tracked = (plant, plant.ToEntity());
            _dbContext.Plants.Attach(tracked.Entity);
        }

        _ = _dbContext.Plants.Remove(tracked.Entity);
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
    public async Task ReplaceWateringHistoryAsync(PlantId plantId, DateOnly? lastWateredOn, CancellationToken cancellationToken)
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

    internal void SyncChanges()
    {
        foreach (var (plant, entity) in _trackedPlants.Values)
        {
            plant.CopyTo(entity);
        }

        foreach (var (location, entity) in _trackedLocations.Values)
        {
            location.CopyTo(entity);
        }
    }

    private Plant Track(PlantEntity entity)
    {
        var plant = entity.ToDomain();
        _trackedPlants[entity.Id] = (plant, entity);
        return plant;
    }

    private Location Track(LocationEntity entity)
    {
        var location = entity.ToDomain();
        foreach (var plantEntity in entity.Plants)
        {
            location.Plants.Add(plantEntity.ToDomain());
        }

        _trackedLocations[entity.Id] = (location, entity);
        return location;
    }

    private readonly GardenDbContext _dbContext;
    private readonly Dictionary<Guid, (Plant Plant, PlantEntity Entity)> _trackedPlants = [];
    private readonly Dictionary<Guid, (Location Location, LocationEntity Entity)> _trackedLocations = [];
}
