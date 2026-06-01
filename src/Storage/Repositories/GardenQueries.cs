using Abstractions.Repositories;

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
    public GardenQueries(
        IPublicGardenQueries publicGardenQueries,
        IGardenPlantQueries gardenPlantQueries,
        IPlantNoteQueries plantNoteQueries,
        IPlantHistoryQueries plantHistoryQueries)
    {
        ArgumentNullException.ThrowIfNull(publicGardenQueries);
        ArgumentNullException.ThrowIfNull(gardenPlantQueries);
        ArgumentNullException.ThrowIfNull(plantNoteQueries);
        ArgumentNullException.ThrowIfNull(plantHistoryQueries);
        _publicGardenQueries = publicGardenQueries;
        _gardenPlantQueries = gardenPlantQueries;
        _plantNoteQueries = plantNoteQueries;
        _plantHistoryQueries = plantHistoryQueries;
    }

    /// <inheritdoc />
    public Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken) =>
        _publicGardenQueries.GetPublicGardenAsync(userId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> PublicPlantExistsAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        _publicGardenQueries.PublicPlantExistsAsync(userId, plantId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken) =>
        _gardenPlantQueries.ListPlantsAsync(userId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(
        UserId userId,
        DateOnly today,
        CancellationToken cancellationToken) =>
        _gardenPlantQueries.ListPlantsAsync(userId, today, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken) =>
        _gardenPlantQueries.ListLocationsAsync(userId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<PlantNoteProjection>> ListPlantNotesAsync(
        UserId userId,
        PlantId plantId,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        _plantNoteQueries.ListPlantNotesAsync(userId, plantId, skip, take, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<PlantNoteProjection>> ListOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        _plantNoteQueries.ListOverduePlantRemindersAsync(userId, plantId, today, skip, take, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<PlantHistoryItemProjection>> ListPlantHistoryAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken) =>
        _plantHistoryQueries.ListPlantHistoryAsync(userId, plantId, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountPlantNotesAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        _plantNoteQueries.CountPlantNotesAsync(userId, plantId, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        CancellationToken cancellationToken) =>
        _plantNoteQueries.CountOverduePlantRemindersAsync(userId, plantId, today, cancellationToken);

    private readonly IPublicGardenQueries _publicGardenQueries;
    private readonly IGardenPlantQueries _gardenPlantQueries;
    private readonly IPlantNoteQueries _plantNoteQueries;
    private readonly IPlantHistoryQueries _plantHistoryQueries;
}
