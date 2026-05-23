using Abstractions.Repositories;

using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core garden read queries.
/// </summary>
public sealed class GardenQueries : IGardenQueries
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GardenQueries"/> class.
    /// </summary>
    public GardenQueries(GardenDbContext dbContext)
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

    private readonly GardenDbContext _dbContext;
}
