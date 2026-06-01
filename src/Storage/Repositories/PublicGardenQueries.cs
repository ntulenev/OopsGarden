using Microsoft.EntityFrameworkCore;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides public garden read queries.
/// </summary>
internal sealed class PublicGardenQueries
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicGardenQueries"/> class.
    /// </summary>
    public PublicGardenQueries(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken)
    {
        var garden = await _dbContext.Users
            .AsNoTracking()
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
                        plant.Soil,
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
                        plant.Soil,
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
    public Task<bool> PublicPlantExistsAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        _dbContext.Plants
            .AsNoTracking()
            .AnyAsync(
                plant =>
                    plant.Id == plantId.Value &&
                    plant.UserId == userId.Value &&
                    plant.User!.IsGardenPublic &&
                    !plant.User.IsBlocked,
                cancellationToken);

    private readonly GardenDbContext _dbContext;
}
