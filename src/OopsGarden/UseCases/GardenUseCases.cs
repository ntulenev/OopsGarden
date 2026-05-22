using Abstractions;

using Models;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="IGetPublicGardenUseCase" />
internal sealed class GetPublicGardenUseCase : IGetPublicGardenUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPublicGardenUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public GetPublicGardenUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="IListGardenPlantsUseCase" />
internal sealed class ListGardenPlantsUseCase : IListGardenPlantsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListGardenPlantsUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListGardenPlantsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="IListGardenLocationsUseCase" />
internal sealed class ListGardenLocationsUseCase : IListGardenLocationsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListGardenLocationsUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListGardenLocationsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocationSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var locations = await _unitOfWork.Garden.ListLocationsAsync(userId, cancellationToken).ConfigureAwait(false);
        return [.. locations
            .Select(location => new LocationSummary(location.Id, location.Name, location.Plants))
        ];
    }

    private readonly IUnitOfWork _unitOfWork;
}

/// <inheritdoc cref="ICreateLocationUseCase" />
internal sealed class CreateLocationUseCase : ICreateLocationUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateLocationUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public CreateLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="IRenameLocationUseCase" />
internal sealed class RenameLocationUseCase : IRenameLocationUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameLocationUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public RenameLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="IDeleteLocationUseCase" />
internal sealed class DeleteLocationUseCase : IDeleteLocationUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLocationUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeleteLocationUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="ICreatePlantUseCase" />
internal sealed class CreatePlantUseCase : ICreatePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public CreatePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="IUpdatePlantUseCase" />
internal sealed class UpdatePlantUseCase : IUpdatePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public UpdatePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="IDeletePlantUseCase" />
internal sealed class DeletePlantUseCase : IDeletePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeletePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <inheritdoc cref="IWaterPlantUseCase" />
internal sealed class WaterPlantUseCase : IWaterPlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaterPlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public WaterPlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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

/// <summary>
/// Maps garden persistence projections to application models.
/// </summary>
internal static class GardenUseCaseMapping
{
    /// <summary>
    /// Converts a garden plant location projection to an application model.
    /// </summary>
    /// <param name="location">The optional location projection.</param>
    /// <returns>The optional garden plant location model.</returns>
    public static GardenPlantLocation? ToResponse(GardenPlantLocationProjection? location) =>
        location is null ? null : new GardenPlantLocation(location.Id, location.Name);

    /// <summary>
    /// Resolves and validates a plant location id for a user.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="userId">The owner user id.</param>
    /// <param name="id">The optional requested location id.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The resolved location result.</returns>
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

/// <summary>
/// Represents the result of resolving a requested location id.
/// </summary>
/// <param name="LocationId">The resolved location id.</param>
/// <param name="Error">The validation error when the location cannot be resolved.</param>
internal sealed record ResolveLocationResult(LocationId? LocationId, string? Error)
{
    /// <summary>
    /// Gets a value indicating whether the location was resolved successfully.
    /// </summary>
    public bool IsSuccess => Error is null;
}
