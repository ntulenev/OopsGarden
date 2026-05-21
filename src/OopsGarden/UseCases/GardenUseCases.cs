using Abstractions;

using Models;

namespace OopsGarden.UseCases;

internal sealed class GetPublicGardenUseCase : IGetPublicGardenUseCase
{
    public GetPublicGardenUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<PublicGarden?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var garden = await _unitOfWork.Garden
            .GetPublicGardenAsync(UserId.From(id), cancellationToken)
            .ConfigureAwait(false);

        return garden is null
            ? null
            : new PublicGarden(
                garden.Id,
                garden.Name,
                garden.Avatar,
                [.. garden.Plants
                    .Select(plant => new PublicGardenPlant(
                        plant.Id,
                        plant.Name,
                        plant.Description,
                        plant.PhotoData,
                        GardenUseCaseMapping.ToResponse(plant.Location)))]);
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class ListGardenPlantsUseCase : IListGardenPlantsUseCase
{
    public ListGardenPlantsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PlantSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var plants = await _unitOfWork.Garden.ListPlantsAsync(userId, cancellationToken).ConfigureAwait(false);
        return [.. plants
            .Select(plant => new PlantSummary(
                plant.Id,
                plant.Name,
                plant.Description,
                plant.PhotoData,
                plant.PlantedOn,
                GardenUseCaseMapping.ToResponse(plant.Location),
                plant.LastWateredAt))];
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class ListGardenLocationsUseCase : IListGardenLocationsUseCase
{
    public ListGardenLocationsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LocationSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var locations = await _unitOfWork.Garden.ListLocationsAsync(userId, cancellationToken).ConfigureAwait(false);
        return [.. locations
            .Select(location => new LocationSummary(location.Id, location.Name, location.Plants))
        ];
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class CreateLocationUseCase : ICreateLocationUseCase
{
    public CreateLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<LocationSummary> ExecuteAsync(
        UserId userId,
        LocationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var location = Location.Create(userId, LocationName.From(command.Name));
        await _unitOfWork.Garden.AddLocationAsync(location, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new LocationSummary(location.Id, location.Name.Value, 0);
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class RenameLocationUseCase : IRenameLocationUseCase
{
    public RenameLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<LocationSummary?> ExecuteAsync(
        UserId userId,
        Guid id,
        LocationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var location = await _unitOfWork.Garden
            .FindLocationAsync(userId, LocationId.From(id), cancellationToken)
            .ConfigureAwait(false);
        if (location is null)
        {
            return null;
        }

        location.Rename(LocationName.From(command.Name));
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new LocationSummary(location.Id, location.Name.Value, location.Plants.Count);
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class DeleteLocationUseCase : IDeleteLocationUseCase
{
    public DeleteLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken)
    {
        var locationId = LocationId.From(id);
        var location = await _unitOfWork.Garden.FindLocationAsync(userId, locationId, cancellationToken).ConfigureAwait(false);
        if (location is null)
        {
            return false;
        }

        await _unitOfWork.Garden.ClearPlantLocationAsync(userId, locationId, cancellationToken).ConfigureAwait(false);
        _unitOfWork.Garden.RemoveLocation(location);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class CreatePlantUseCase : ICreatePlantUseCase
{
    public CreatePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePlantResult> ExecuteAsync(
        UserId userId,
        PlantCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var locationResult = await GardenUseCaseMapping
            .ResolveLocationIdAsync(_unitOfWork, userId, command.LocationId, cancellationToken)
            .ConfigureAwait(false);
        if (!locationResult.IsSuccess)
        {
            return new CreatePlantResult(null, locationResult.Error);
        }

        var plant = Plant.Create(
            userId,
            PlantName.From(command.Name),
            PlantDescription.From(command.Description),
            locationResult.LocationId,
            command.PlantedOn,
            command.PhotoData);
        await _unitOfWork.Garden.AddPlantAsync(plant, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new CreatePlantResult(plant.Id.Value, null);
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class UpdatePlantUseCase : IUpdatePlantUseCase
{
    public UpdatePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdatePlantResult> ExecuteAsync(
        UserId userId,
        Guid id,
        PlantCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var plantId = PlantId.From(id);
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, plantId, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return new UpdatePlantResult(UpdatePlantStatus.NotFound, null);
        }

        var locationResult = await GardenUseCaseMapping
            .ResolveLocationIdAsync(_unitOfWork, userId, command.LocationId, cancellationToken)
            .ConfigureAwait(false);
        if (!locationResult.IsSuccess)
        {
            return new UpdatePlantResult(UpdatePlantStatus.Invalid, locationResult.Error);
        }

        plant.UpdateDetails(
            PlantName.From(command.Name),
            PlantDescription.From(command.Description),
            locationResult.LocationId,
            command.PlantedOn,
            command.PhotoData);
        await _unitOfWork.Garden.ReplaceWateringHistoryAsync(plantId, command.LastWateredOn, cancellationToken)
            .ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new UpdatePlantResult(UpdatePlantStatus.Updated, null);
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class DeletePlantUseCase : IDeletePlantUseCase
{
    public DeletePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, PlantId.From(id), cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return false;
        }

        _unitOfWork.Garden.RemovePlant(plant);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class WaterPlantUseCase : IWaterPlantUseCase
{
    public WaterPlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<DateTimeOffset?> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken)
    {
        var plant = await _unitOfWork.Garden.FindPlantAsync(userId, PlantId.From(id), cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return null;
        }

        var watering = plant.Water();
        await _unitOfWork.Garden.AddWateringEventAsync(watering, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return watering.WateredAt;
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal static class GardenUseCaseMapping
{
    public static GardenPlantLocation? ToResponse(GardenPlantLocationProjection? location) =>
        location is null ? null : new GardenPlantLocation(location.Id, location.Name);

    public static async Task<ResolveLocationResult> ResolveLocationIdAsync(
        IUnitOfWork unitOfWork,
        UserId userId,
        Guid? id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        if (!id.HasValue)
        {
            return new ResolveLocationResult(null, null);
        }

        var locationId = LocationId.From(id.Value);
        return await unitOfWork.Garden.LocationExistsAsync(userId, locationId, cancellationToken).ConfigureAwait(false)
            ? new ResolveLocationResult(locationId, null)
            : new ResolveLocationResult(null, "Invalid location.");
    }
}

internal sealed record ResolveLocationResult(LocationId? LocationId, string? Error)
{
    public bool IsSuccess => Error is null;
}
