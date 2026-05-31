using Abstractions.Repositories;

using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core plant persistence operations.
/// </summary>
public sealed class PlantRepository : IPlantRepository, ISyncChanges
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlantRepository"/> class.
    /// </summary>
    public PlantRepository(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
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
    public Task AddPlantAsync(Plant plant, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plant);
        var entity = plant.ToEntity();
        _ = _dbContext.Plants.Add(entity);
        _trackedPlants[plant.Id.Value] = (plant, entity);
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
    public Task AddPlantNoteAsync(PlantNote note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(note);
        _ = _dbContext.PlantNotes.Add(note.ToEntity());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AddPlantPhotoAsync(
        PlantId plantId,
        ImageDataUrl photoDataUrl,
        DateTimeOffset uploadedAt,
        CancellationToken cancellationToken)
    {
        _ = _dbContext.PlantPhotos.Add(new Entities.PlantPhotoEntity
        {
            Id = Guid.NewGuid(),
            PlantId = plantId.Value,
            PhotoData = photoDataUrl.Value,
            UploadedAt = uploadedAt
        });
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
    public async Task<bool> UpdatePlantNoteCreatedAtAsync(
        UserId userId,
        PlantId plantId,
        PlantNoteId noteId,
        DateTimeOffset createdAt,
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

        note.CreatedAt = createdAt;
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveWateringEventAsync(
        UserId userId,
        PlantId plantId,
        WateringEventId wateringEventId,
        CancellationToken cancellationToken)
    {
        var watering = await _dbContext.WateringEvents
            .SingleOrDefaultAsync(
                watering =>
                    watering.Id == wateringEventId.Value &&
                    watering.PlantId == plantId.Value &&
                    watering.Plant!.UserId == userId.Value,
                cancellationToken)
            .ConfigureAwait(false);
        if (watering is null)
        {
            return false;
        }

        _ = _dbContext.WateringEvents.Remove(watering);
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
    public void SyncChanges()
    {
        foreach (var (plant, entity) in _trackedPlants.Values)
        {
            plant.CopyTo(entity);
        }
    }

    private Plant Track(Entities.PlantEntity entity)
    {
        var plant = entity.ToDomain();
        _trackedPlants[entity.Id] = (plant, entity);
        return plant;
    }

    private readonly GardenDbContext _dbContext;
    private readonly Dictionary<Guid, (Plant Plant, Entities.PlantEntity Entity)> _trackedPlants = [];
}
