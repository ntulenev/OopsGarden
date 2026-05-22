using Abstractions;

using Models;

namespace OopsGarden.Tests;

internal sealed class FakeGardenRepository : IGardenRepository
{
    public List<Location> Locations { get; } = [];

    public List<Plant> Plants { get; } = [];

    public List<WateringEvent> WateringEvents { get; } = [];

    public Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult<PublicGardenProjection?>(new PublicGardenProjection(userId, "User", null, []));

    public Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<GardenPlantProjection>>([.. Plants
            .Where(plant => plant.UserId == userId)
            .Select(plant => new GardenPlantProjection(
                plant.Id,
                plant.Name.Value,
                plant.Description.Value,
                plant.PhotoDataUrl?.Value,
                plant.PlantedOn,
                null,
                WateringEvents.LastOrDefault(watering => watering.PlantId == plant.Id)?.WateredAt))]);

    public Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<GardenLocationProjection>>([.. Locations
            .Where(location => location.UserId == userId)
            .Select(location => new GardenLocationProjection(location.Id, location.Name.Value, location.Plants.Count))]);

    public Task<Plant?> FindPlantAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        Task.FromResult(Plants.SingleOrDefault(plant => plant.UserId == userId && plant.Id == plantId));

    public Task<Location?> FindLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken) =>
        Task.FromResult(Locations.SingleOrDefault(location => location.UserId == userId && location.Id == locationId));

    public Task<bool> LocationExistsAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken) =>
        Task.FromResult(Locations.Exists(location => location.UserId == userId && location.Id == locationId));

    public Task AddPlantAsync(Plant plant, CancellationToken cancellationToken)
    {
        Plants.Add(plant);
        return Task.CompletedTask;
    }

    public Task AddLocationAsync(Location location, CancellationToken cancellationToken)
    {
        Locations.Add(location);
        return Task.CompletedTask;
    }

    public Task AddWateringEventAsync(WateringEvent watering, CancellationToken cancellationToken)
    {
        WateringEvents.Add(watering);
        return Task.CompletedTask;
    }

    public void RemovePlant(Plant plant) => Plants.Remove(plant);

    public void RemoveLocation(Location location) => Locations.Remove(location);

    public Task ClearPlantLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken)
    {
        foreach (var plant in Plants.Where(plant => plant.UserId == userId && plant.LocationId == locationId))
        {
            plant.UpdateDetails(plant.Name, plant.Description, null, plant.PlantedOn, plant.PhotoDataUrl?.Value);
        }

        return Task.CompletedTask;
    }

    public Task ReplaceWateringHistoryAsync(PlantId plantId, DateOnly? lastWateredOn, CancellationToken cancellationToken)
    {
        WateringEvents.RemoveAll(watering => watering.PlantId == plantId);
        if (lastWateredOn.HasValue)
        {
            WateringEvents.Add(WateringEvent.Restore(
                WateringEventId.New(),
                plantId,
                new DateTimeOffset(lastWateredOn.Value.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero)));
        }

        return Task.CompletedTask;
    }
}
